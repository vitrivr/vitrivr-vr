using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Interface between the <see cref="QueryController"/> and the individual <see cref="QueryTermProvider"/>s.
  /// Manages the <see cref="QueryTerm"/>s into stages and temporal contexts.
  /// </summary>
  public class QueryTermManager : MonoBehaviour
  {
    /// <summary>
    /// Space between representations within a stage.
    /// </summary>
    public float representationSpacing = 0.05f;

    /// <summary>
    /// Space between stages within a temporal context.
    /// </summary>
    public float stageSpacing = 0.1f;

    /// <summary>
    /// Space between temporal contexts.
    /// </summary>
    public float temporalSpacing = 0.2f;

    public QueryTermProviderRepresentation queryTermProviderRepresentationPrefab;
    public Transform newTemporalIndicator0, newTemporalIndicator1;
    public Transform newStageIndicator0, newStageIndicator1;

    private readonly List<List<List<(QueryTermProvider provider, QueryTermProviderRepresentation representation)>>>
      _queryTermProviders = new();

    /// <summary>
    /// The number of term providers that are currently being reordered.
    /// </summary>
    private int _currentlyReordering;

    /// <summary>
    /// Retrieves the <see cref="QueryTerm"/>s sorted into stages and temporal contexts.
    /// </summary>
    /// <returns>List of temporal contexts containing lists of stages containing lists of terms.</returns>
    public List<List<List<QueryTerm>>> GetTerms()
    {
      var terms = _queryTermProviders
        .Select(temporal => temporal
          .Select(stages => stages
            .SelectMany(tuple => tuple.provider.GetTerms()).ToList()
          ).ToList()
        ).ToList();

      terms.ForEach(temporal => temporal.RemoveAll(stage => stage.Count == 0));
      terms.RemoveAll(temporal => temporal.Count == 0);
      return terms;
    }

    public void Add(QueryTermProvider termProvider)
    {
      var representation = Instantiate(queryTermProviderRepresentationPrefab);
      var (providerName, n) = GetNewName(termProvider);
      representation.Initialize(this, termProvider, Vector3.up * 0.1f, providerName, n);

      // No temporal contexts yet
      if (_queryTermProviders.Count == 0)
      {
        _queryTermProviders.Add(new List<List<(QueryTermProvider, QueryTermProviderRepresentation)>>
          {new() {(termProvider, representation)}});
        return;
      }

      // A temporal context cannot exist without at least a single stage, so we can safely assume that this exists
      _queryTermProviders.Last().Last().Add((termProvider, representation));

      UpdateOffsets();
    }

    public void Remove(QueryTermProvider termProvider)
    {
      foreach (var stage in _queryTermProviders.SelectMany(temporal => temporal))
      {
        stage.RemoveAll(tuple =>
        {
          if (tuple.provider != termProvider && tuple.provider != null) return false;
          Destroy(tuple.representation.gameObject);
          return true;
        });
      }

      // Remove empty stages and temporal contexts
      RemoveEmpty();

      UpdateOffsets();
    }

    /// <summary>
    /// Reorganize the query term providers by how the provided representation is positioned.
    /// </summary>
    /// <param name="representation">The representation to reorganize.</param>
    /// <param name="position">Place in the old organization to position the representation.</param>
    public void Reorganize(QueryTermProviderRepresentation representation, float position)
    {
      // No reorganization possible if completely empty or only one item
      if (_queryTermProviders.Count == 0 ||
          (_queryTermProviders.Count == 1 && _queryTermProviders.First().Count == 1 &&
           _queryTermProviders.First().First().Count == 1))
        return;

      if (position < newTemporalIndicator0.localPosition.x)
      {
        var termProvider = Remove(representation);
        _queryTermProviders.Insert(0,
          new List<List<(QueryTermProvider, QueryTermProviderRepresentation)>>
            {new() {(termProvider, representation)}});
      }
      else if (position < newStageIndicator0.localPosition.x)
      {
        var termProvider = Remove(representation);
        _queryTermProviders.First().Insert(0,
          new List<(QueryTermProvider, QueryTermProviderRepresentation)>
            {(termProvider, representation)});
      }
      else if (position > newTemporalIndicator1.localPosition.x)
      {
        var termProvider = Remove(representation);
        _queryTermProviders.Add(
          new List<List<(QueryTermProvider, QueryTermProviderRepresentation)>>
            {new() {(termProvider, representation)}});
      }
      else if (position > newStageIndicator1.localPosition.x)
      {
        var termProvider = Remove(representation);
        _queryTermProviders.Last().Add(
          new List<(QueryTermProvider provider, QueryTermProviderRepresentation representation)>
            {(termProvider, representation)});
      }
      else
      {
        // Dropped somewhere in between
        ReorganizeInside(representation, position);
      }

      UpdateOffsets();
    }

    public void StartReordering()
    {
      _currentlyReordering += 1;
      if (_currentlyReordering != 1) return;
      newStageIndicator0.gameObject.SetActive(true);
      newStageIndicator1.gameObject.SetActive(true);
      newTemporalIndicator0.gameObject.SetActive(true);
      newTemporalIndicator1.gameObject.SetActive(true);
      UpdateOffsets();
    }

    public void EndReordering()
    {
      _currentlyReordering -= 1;
      if (_currentlyReordering >= 1) return;
      newStageIndicator0.gameObject.SetActive(false);
      newStageIndicator1.gameObject.SetActive(false);
      newTemporalIndicator0.gameObject.SetActive(false);
      newTemporalIndicator1.gameObject.SetActive(false);
      UpdateOffsets();
    }

    private (string, int) GetNewName(QueryTermProvider termProvider)
    {
      var baseName = termProvider.GetTypeName();

      var maxN = _queryTermProviders.SelectMany(
          temporal => temporal.SelectMany(
            stage => stage.Select(pair => pair.representation)
          )
        ).Where(rep => rep.TypeName.Equals(baseName))
        .Select(rep => rep.N)
        .DefaultIfEmpty().Max();

      return (baseName, maxN + 1);
    }

    private (int, int) GetStageIndex(QueryTermProviderRepresentation representation)
    {
      for (var i = 0; i < _queryTermProviders.Count; i++)
      {
        for (var j = 0; j < _queryTermProviders[i].Count; j++)
        {
          for (var k = 0; k < _queryTermProviders[i][j].Count; k++)
          {
            if (_queryTermProviders[i][j][k].representation == representation)
            {
              return (i, j);
            }
          }
        }
      }

      return (-1, -1);
    }

    private void ReorganizeInside(QueryTermProviderRepresentation representation, float position)
    {
      var (i, j) = GetStageIndex(representation);

      var width = CalculateWidth();

      var x = -width / 2;

      foreach (var (temporal, ti) in _queryTermProviders.Select((temporal, ti) => (temporal, ti)))
      {
        foreach (var (stage, si) in temporal.Select((stage, si) => (stage, si)))
        {
          var stageWidth = stage.Count * representationSpacing;
          var stageAreaStart = x - representationSpacing / 2;

          if (position > stageAreaStart && position < stageAreaStart + stageWidth)
          {
            // Term provider already in this stage
            if (ti == i && si == j)
              return;

            var termProvider = Remove(representation);
            stage.Add((termProvider, representation));
            return;
          }

          x += stageSpacing + stageWidth - representationSpacing;
        }

        x += temporalSpacing - stageSpacing;
      }
    }

    private QueryTermProvider Remove(QueryTermProviderRepresentation representation)
    {
      foreach (var stage in _queryTermProviders.SelectMany(temporal => temporal))
      {
        var valueTuple = stage.Find(tuple => tuple.representation == representation);

        if (valueTuple == (null, null)) continue;
        stage.Remove(valueTuple);
        // Remove empty stages and temporal contexts
        RemoveEmpty();

        UpdateOffsets();

        return valueTuple.provider;
      }

      return null;
    }

    /// <summary>
    /// Removes empty stages and empty temporal contexts.
    /// </summary>
    private void RemoveEmpty()
    {
      foreach (var temporal in _queryTermProviders)
      {
        temporal.RemoveAll(stage => stage.Count == 0);
      }

      _queryTermProviders.RemoveAll(temporal => temporal.Count == 0);
    }

    /// <summary>
    /// Calculate total width of the query term provider representations.
    /// </summary>
    /// <returns>The width of the representation in world units.</returns>
    private float CalculateWidth()
    {
      return (_queryTermProviders.Count - 1) * temporalSpacing + _queryTermProviders.Sum(
        temporal => (temporal.Count - 1) * stageSpacing + temporal.Sum(
          stage => (stage.Count - 1) * representationSpacing
        )
      );
    }

    /// <summary>
    /// Updates the offsets (positions if not grabbed) of all representations (and the four indicators if appropriate).
    /// </summary>
    private void UpdateOffsets()
    {
      // Calculate total width
      var width = CalculateWidth();

      var halfWidth = width / 2;

      // Position representations
      var x = 0f;

      foreach (var temporal in _queryTermProviders)
      {
        foreach (var stage in temporal)
        {
          foreach (var (_, representation) in stage)
          {
            representation.UpdateOffset(x - halfWidth);
            x += representationSpacing;
          }

          x += stageSpacing - representationSpacing;
        }

        x += temporalSpacing - stageSpacing;
      }

      if (_currentlyReordering < 1) return;
      newStageIndicator0.transform.localPosition = new Vector3(-halfWidth - stageSpacing, 0.1f, 0);
      newStageIndicator1.transform.localPosition = new Vector3(halfWidth + stageSpacing, 0.1f, 0);
      newTemporalIndicator0.transform.localPosition = new Vector3(-halfWidth - stageSpacing - temporalSpacing, 0.1f, 0);
      newTemporalIndicator1.transform.localPosition = new Vector3(halfWidth + stageSpacing + temporalSpacing, 0.1f, 0);
    }
  }
}
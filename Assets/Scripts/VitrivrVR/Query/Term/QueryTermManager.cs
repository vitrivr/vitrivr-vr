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
    public float representationSpacing = 0.05f;
    public float stageSpacing = 0.1f;
    public float temporalSpacing = 0.2f;
    public QueryTermProviderRepresentation queryTermProviderRepresentationPrefab;

    private readonly List<List<List<(QueryTermProvider provider, QueryTermProviderRepresentation representation)>>>
      _queryTermProviders = new();

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
      representation.Initialize(this, termProvider, Vector3.up * 0.1f);

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
      foreach (var temporal in _queryTermProviders)
      {
        temporal.RemoveAll(stage => stage.Count == 0);
      }

      _queryTermProviders.RemoveAll(temporal => temporal.Count == 0);
      UpdateOffsets();
    }

    private void UpdateOffsets()
    {
      // Calculate total width
      var width = (_queryTermProviders.Count - 1) * temporalSpacing + _queryTermProviders.Sum(
        temporal => (temporal.Count - 1) * stageSpacing + temporal.Sum(
          stage => (stage.Count - 1) * representationSpacing
        )
      );

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
    }
  }
}
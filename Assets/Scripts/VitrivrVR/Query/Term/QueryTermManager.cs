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
    private List<List<List<QueryTermProvider>>> _queryTermProviders = new();

    /// <summary>
    /// Retrieves the <see cref="QueryTerm"/>s sorted into stages and temporal contexts.
    /// </summary>
    /// <returns>List of temporal contexts containing lists of stages containing lists of terms.</returns>
    public List<List<List<QueryTerm>>> GetTerms()
    {
      var terms = _queryTermProviders
        .Select(temporal => temporal
          .Select(stages => stages
            .SelectMany(termProvider => termProvider.GetTerms()).ToList()
          ).ToList()
        ).ToList();

      terms.ForEach(temporal => temporal.RemoveAll(stage => stage.Count == 0));
      terms.RemoveAll(temporal => temporal.Count == 0);
      return terms;
    }

    public void Add(QueryTermProvider termProvider)
    {
      // No temporal contexts yet
      if (_queryTermProviders.Count == 0)
      {
        _queryTermProviders.Add(new List<List<QueryTermProvider>> {new() {termProvider}});
        return;
      }

      // A temporal context cannot exist without at least a single stage, so we can safely assume that this exists
      _queryTermProviders.Last().Last().Add(termProvider);
    }

    public void Remove(QueryTermProvider termProvider)
    {
      foreach (var stage in _queryTermProviders.SelectMany(temporal => temporal))
      {
        stage.Remove(termProvider);
      }

      // Remove empty stages and temporal contexts
      foreach (var temporal in _queryTermProviders)
      {
        temporal.RemoveAll(stage => stage.Count == 0);
      }

      _queryTermProviders.RemoveAll(temporal => temporal.Count == 0);
    }
  }
}
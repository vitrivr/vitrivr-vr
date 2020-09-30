using System;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using VitrivrVR.Media;
using VitrivrVR.Query.Term;

namespace VitrivrVR.Query
{
  public class QueryController : MonoBehaviour
  {
    public QueryTermProvider queryTermProvider;
    public int prefetch = 72;
    public MediaCarouselController mediaCarousel;

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;

    public async void RunQuery()
    {
      var localGuid = Guid.NewGuid();
      _localQueryGuid = localGuid;

      var queryTerms = queryTermProvider.GetTerms();

      if (queryTerms.Count == 0)
      {
        Debug.Log("Cannot run query: No terms specified.");
        return;
      }

      mediaCarousel.ClearResults();

      var query = QueryBuilder.BuildSimilarityQuery(queryTerms.ToArray());

      var queryData = await CineastWrapper.ExecuteQuery(query, 1000, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      mediaCarousel.CreateResults(queryData);
    }
  }
}
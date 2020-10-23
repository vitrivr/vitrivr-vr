using System;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using VitrivrVR.Config;
using VitrivrVR.Query.Display;
using VitrivrVR.Query.Term;

namespace VitrivrVR.Query
{
  /// <summary>
  /// Object for the execution of similarity queries and the creation of query result displays.
  /// </summary>
  public class QueryController : MonoBehaviour
  {
    public QueryTermProvider queryTermProvider;
    public GameObject timer;
    public QueryDisplay queryDisplay;

    private QueryDisplay _currentDisplay;

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

      if (_currentDisplay != null)
      {
        Destroy(_currentDisplay.gameObject);
      }

      var query = QueryBuilder.BuildSimilarityQuery(queryTerms.ToArray());

      if (!timer.activeSelf)
      {
        timer.SetActive(true);
      }

      var config = ConfigManager.Config;
      var maxResults = config.maxResults;
      var prefetch = config.maxPrefetch;
      var queryData = await CineastWrapper.ExecuteQuery(query, maxResults, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      _currentDisplay = Instantiate(queryDisplay);

      _currentDisplay.Initialize(queryData);

      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }
  }
}
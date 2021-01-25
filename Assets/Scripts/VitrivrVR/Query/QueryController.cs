using System;
using System.Collections.Generic;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using VitrivrVR.Config;
using VitrivrVR.Notification;
using VitrivrVR.Query.Display;
using VitrivrVR.Query.Term;

namespace VitrivrVR.Query
{
  /// <summary>
  /// Object for the execution of similarity queries and the creation of query result displays.
  /// </summary>
  public class QueryController : MonoBehaviour
  {
    public static QueryController Instance { get; protected set; }
    
    public QueryTermProvider defaultQueryTermProvider;
    public GameObject timer;
    public QueryDisplay queryDisplay;

    private QueryDisplay _currentDisplay;

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;

    private void Awake()
    {
      if (Instance != null)
      {
        Debug.LogError("Multiple QueryControllers registered!");
      }

      Instance = this;
    }

    public void RunQuery()
    {
      RunQuery(defaultQueryTermProvider);
    }


    public void RunQuery(QueryTermProvider queryTermProvider)
    {
      var queryTerms = queryTermProvider.GetTerms();
      RunQuery(queryTerms);
    }

    public async void RunQuery(List<QueryTerm> queryTerms)
    {
      var localGuid = Guid.NewGuid();
      _localQueryGuid = localGuid;

      if (queryTerms.Count == 0)
      {
        NotificationController.Notify("Cannot run query: No terms specified.");
        return;
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
      
      if (_currentDisplay != null)
      {
        // TODO: Stash / disable existing query instead of destroying it immediately
        Destroy(_currentDisplay.gameObject);
      }

      _currentDisplay = Instantiate(queryDisplay);

      _currentDisplay.Initialize(queryData);

      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }

    public void ClearQuery()
    {
      if (_currentDisplay == null) return;
      Destroy(_currentDisplay.gameObject);
      _currentDisplay = null;
    }
  }
}
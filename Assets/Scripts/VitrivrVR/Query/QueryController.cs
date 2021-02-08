using System;
using System.Collections.Generic;
using System.Linq;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using UnityEngine.Events;
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
    [Serializable]
    public class QueryEvent : UnityEvent<int>
    {
    }

    public static QueryController Instance { get; private set; }

    public QueryTermProvider defaultQueryTermProvider;
    public GameObject timer;
    public QueryDisplay queryDisplay;

    public readonly List<(SimilarityQuery query, QueryDisplay display)> queries =
      new List<(SimilarityQuery, QueryDisplay)>();

    /// <summary>
    /// Event is triggered when a new query is added to the query list. Argument is query index.
    /// </summary>
    public QueryEvent queryAddedEvent;

    /// <summary>
    /// Event is triggered when an existing query is removed from the query list. Argument is query index.
    /// </summary>
    public QueryEvent queryRemovedEvent;

    /// <summary>
    /// Event is triggered when the active query changes. Argument is query index, -1 means no focus.
    /// </summary>
    public QueryEvent queryFocusEvent;

    private int _currentQuery = -1;

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

      if (_currentQuery != -1)
      {
        ClearQuery();
      }

      var display = Instantiate(queryDisplay);

      display.Initialize(queryData);

      queries.Add((query, display));
      _currentQuery = queries.Count - 1;
      queryAddedEvent.Invoke(_currentQuery);
      queryFocusEvent.Invoke(_currentQuery);

      // Query display already created and initialized, but if this is no longer the newest query, do not disable query
      // indicator
      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }

    public void SelectQuery(int index)
    {
      if (0 > index || index >= queries.Count)
      {
        throw new ArgumentException($"Query selection index out of range: {index} (queries: {queries.Count})");
      }

      if (_currentQuery != -1)
      {
        SetQueryActive(_currentQuery, false);
      }

      _currentQuery = index;
      SetQueryActive(_currentQuery, true);
    }

    /// <summary>
    /// Removes the specified query from the query list and destroys the associated QueryDisplay (notifies event
    /// subscribers before removal and destruction).
    /// </summary>
    public void RemoveQuery(SimilarityQuery query)
    {
      var index = queries.Select(pair => pair.query).ToList().IndexOf(query);
      RemoveQuery(index);
    }

    /// <summary>
    /// Removes the query at the specified index of the query list and destroys the associated QueryDisplay (notifies
    /// event subscribers before removal and destruction).
    /// </summary>
    public void RemoveQuery(int index)
    {
      queryRemovedEvent.Invoke(index);
      Destroy(queries[index].display.gameObject);
      queries.RemoveAt(index);
    }

    public void ClearQuery()
    {
      if (_currentQuery == -1) return;
      SetQueryActive(_currentQuery, false);
      _currentQuery = -1;
      queryFocusEvent.Invoke(_currentQuery);
    }

    private void SetQueryActive(int index, bool active)
    {
      queries[index].display.gameObject.SetActive(active);
    }
  }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using UnityEngine.Events;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Config;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;
using VitrivrVR.Logging;
using VitrivrVR.Notification;
using VitrivrVR.Query.Display;
using VitrivrVR.Query.Term;
using static VitrivrVR.Logging.Interaction;

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

    [Serializable]
    public class QueryChangeEvent : UnityEvent<int, int>
    {
    }

    public static QueryController Instance { get; private set; }

    public QueryTermManager defaultQueryTermManager;
    public GameObject timer;
    public QueryDisplay queryDisplay;
    public TemporalQueryDisplay temporalQueryDisplay;

    public readonly List<QueryDisplay> Queries = new();

    public int CurrentQuery { get; private set; } = -1;

    /// <summary>
    /// Event is triggered when a new query is added to the query list. Argument is query index.
    /// </summary>
    public QueryEvent queryAddedEvent;

    /// <summary>
    /// Event is triggered when an existing query is removed from the query list. Argument is query index.
    /// </summary>
    public QueryEvent queryRemovedEvent;

    /// <summary>
    /// Event is triggered when the active query changes. Argument is old query index, new query index, -1 means no
    /// focus.
    /// </summary>
    public QueryChangeEvent queryFocusEvent;

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;

    private CineastClient _cineastClient;

    private void Awake()
    {
      if (Instance != null)
      {
        throw new Exception("Multiple QueryControllers registered!");
      }

      _cineastClient = new CineastClient(CineastConfigManager.LoadConfigOrDefault("cineastapi.json"));

      Instance = this;
    }

    public void RunQuery()
    {
      RunQuery(defaultQueryTermManager);
    }


    public void RunQuery(QueryTermManager queryTermManager)
    {
      var queryTerms = queryTermManager.GetTerms();

      switch (queryTerms.Count)
      {
        case 0:
          NotificationController.Notify("Cannot run query: No terms specified.");
          return;
        // No temporal context specified
        case 1:
        {
          var stages = queryTerms.First();
          // No stages specified
          if (stages.Count == 1)
          {
            RunQuery(stages.First());
            return;
          }

          // With stages
          RunQuery(stages);
          return;
        }
        // With temporal context (more than 1 temporal container)
        default:
          RunQuery(queryTerms);
          break;
      }
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

      var config = ConfigManager.Config;
      var maxResults = config.maxResults;
      var prefetch = config.maxPrefetch;

      var query = QueryBuilder.BuildSimilarityQuery(queryTerms.ToArray());
      // TODO: Move to QueryBuilder
      query.Config = new QueryConfig(resultsPerModule: maxResults);

      if (!timer.activeSelf)
      {
        timer.SetActive(true);
      }

      var queryData = await _cineastClient.ExecuteQuery(query, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      InstantiateQueryDisplay(queryData);

      // Query display already created and initialized, but if this is no longer the newest query, do not disable query
      // indicator
      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }

    public async void RunQuery(List<List<QueryTerm>> stages)
    {
      var localGuid = Guid.NewGuid();
      _localQueryGuid = localGuid;

      var config = ConfigManager.Config;
      var maxResults = config.maxResults;
      var prefetch = config.maxPrefetch;

      var query = QueryBuilder.BuildStagedQuery(stages);
      // TODO: Move to QueryBuilder
      query.Config = new QueryConfig(resultsPerModule: maxResults);

      if (!timer.activeSelf)
      {
        timer.SetActive(true);
      }

      var queryData = await _cineastClient.ExecuteQuery(query, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      InstantiateQueryDisplay(queryData);

      // Query display already created and initialized, but if this is no longer the newest query, do not disable query
      // indicator
      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }

    public async void RunQuery(List<List<List<QueryTerm>>> temporalTerms)
    {
      var localGuid = Guid.NewGuid();
      _localQueryGuid = localGuid;

      var config = ConfigManager.Config;
      var maxResults = config.maxResults;
      var prefetch = config.maxPrefetch;

      var query = QueryBuilder.BuildTemporalQuery(temporalTerms);
      // TODO: Move to QueryBuilder
      query.Config = new TemporalQueryConfig(maxResults: maxResults);

      if (!timer.activeSelf)
      {
        timer.SetActive(true);
      }

      var queryData = await _cineastClient.ExecuteQuery(query, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      InstantiateQueryDisplay(queryData);

      // Query display already created and initialized, but if this is no longer the newest query, do not disable query
      // indicator
      if (_localQueryGuid != localGuid) return;
      timer.transform.localRotation = Quaternion.identity;
      timer.SetActive(false);
    }

    public void SelectQuery(QueryDisplay display)
    {
      var index = Queries.IndexOf(display);
      SelectQuery(index);
    }

    public void SelectQuery(int index)
    {
      if (0 > index || index >= Queries.Count)
      {
        throw new ArgumentException($"Query selection index out of range: {index} (queries: {Queries.Count})");
      }

      if (CurrentQuery != -1)
      {
        SetQueryActive(CurrentQuery, false);
      }

      SetQueryActive(index, true);
      queryFocusEvent.Invoke(CurrentQuery, index);
      CurrentQuery = index;

      LoggingController.LogInteraction("queryManagement", $"select {index}", QueryManagement);
    }

    /// <summary>
    /// Removes the specified query display from the query list and destroys the associated QueryDisplay (notifies event
    /// subscribers before removal and destruction).
    /// </summary>
    public void RemoveQuery(QueryDisplay display)
    {
      var index = Queries.IndexOf(display);
      RemoveQuery(index);
    }

    /// <summary>
    /// Removes the query at the specified index of the query list and destroys the associated QueryDisplay (notifies
    /// event subscribers before removal and destruction).
    /// </summary>
    public void RemoveQuery(int index)
    {
      if (0 > index || index >= Queries.Count)
      {
        throw new ArgumentException($"Query selection index out of range: {index} (queries: {Queries.Count})");
      }

      if (index == CurrentQuery)
      {
        ClearQuery();
      }

      if (index < CurrentQuery)
      {
        CurrentQuery--;
      }

      queryRemovedEvent.Invoke(index);
      Destroy(Queries[index].gameObject);
      Queries.RemoveAt(index);
      LoggingController.LogInteraction("queryManagement", $"delete {index}", QueryManagement);
    }

    public void RemoveAllQueries()
    {
      for (var queryIndex = Queries.Count - 1; queryIndex >= 0; queryIndex--)
      {
        RemoveQuery(queryIndex);
      }
    }

    public void ClearQuery()
    {
      if (CurrentQuery == -1) return;
      SetQueryActive(CurrentQuery, false);
      queryFocusEvent.Invoke(CurrentQuery, -1);
      CurrentQuery = -1;
      LoggingController.LogInteraction("queryManagement", "clear", QueryManagement);
    }

    /// <summary>
    /// Creates a new query display from the current active query results.
    /// </summary>
    public void NewDisplayFromActive()
    {
      if (CurrentQuery == -1)
      {
        NotificationController.Notify("No query selected!");
        return;
      }

      var display = Queries[CurrentQuery];
      if (display.GetType() == queryDisplay.GetType())
      {
        NotificationController.Notify($"Current query display already of type {display.GetType().Name}!");
        return;
      }

      InstantiateQueryDisplay(display.QueryData);
    }

    private void SetQueryActive(int index, bool active)
    {
      Queries[index].gameObject.SetActive(active);
    }

    private void InstantiateQueryDisplay(QueryResponse queryData)
    {
      if (CurrentQuery != -1)
      {
        ClearQuery();
      }

      var display = Instantiate(queryDisplay);

      display.Initialize(queryData);

      Queries.Add(display);
      var queryIndex = Queries.Count - 1;
      queryAddedEvent.Invoke(queryIndex);
      queryFocusEvent.Invoke(CurrentQuery, queryIndex);
      CurrentQuery = queryIndex;
    }

    public SegmentData GetSegment(string segmentId)
    {
      return _cineastClient.MultimediaRegistry.GetSegment(segmentId);
    }

    public CineastConfig GetCineastConfig()
    {
      return _cineastClient.CineastConfig;
    }

    public async Task<List<string>> GetDistinctTableValues(string table, string column)
    {
      return await _cineastClient.GetDistinctTableValues(table, column);
    }

    public async Task<List<Tag>> GetMatchingTags(string tagName)
    {
      return await _cineastClient.GetMatchingTags(tagName);
    }

    private void InstantiateQueryDisplay(TemporalQueryResponse queryData)
    {
      if (CurrentQuery != -1)
      {
        ClearQuery();
      }

      var display = Instantiate(temporalQueryDisplay);

      display.Initialize(queryData);

      Queries.Add(display);
      var queryIndex = Queries.Count - 1;
      queryAddedEvent.Invoke(queryIndex);
      queryFocusEvent.Invoke(CurrentQuery, queryIndex);
      CurrentQuery = queryIndex;
    }
  }
}
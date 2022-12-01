using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dev.Dres.ClientApi.Model;
using Dres.Unityclient;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Config;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Config;
using VitrivrVR.Logging;
using VitrivrVR.Notification;

namespace VitrivrVR.Submission
{
  public class DresClientManager : MonoBehaviour
  {
    private static DresClient _instance;

    private static readonly List<QueryEvent> InteractionEvents = new();
    private static float _interactionEventTimer;

    private async void Start()
    {
      if (!ConfigManager.Config.dresEnabled) return;

      if (ConfigManager.Config.allowInvalidCertificate)
      {
        ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
        // (sender, certificate, chain, sslPolicyErrors) => true;
      }

      _instance = new DresClient();
      await _instance.Login();
      var username = _instance.UserDetails.Username;
      NotificationController.Notify($"DRES connected: {username}");
    }

    private void Update()
    {
      if (!ConfigManager.Config.dresEnabled) return;

      //Update timer
      _interactionEventTimer += Time.deltaTime;
      if (!(_interactionEventTimer > ConfigManager.Config.interactionLogSubmissionInterval)) return;
      // Reset timer
      _interactionEventTimer %= ConfigManager.Config.interactionLogSubmissionInterval;
      LogInteraction();
    }

    private void OnDestroy()
    {
      if (!ConfigManager.Config.dresEnabled) return;
      LogInteraction();
    }

    public static async void SubmitResult(string mediaObjectId, int? frame = null)
    {
      mediaObjectId = RemovePrefix(mediaObjectId);

      try
      {
        var result = await _instance.SubmitResult(mediaObjectId, frame);
        NotificationController.Notify($"Submission: {result.Submission}");
      }
      catch (Exception e)
      {
        NotificationController.Notify(e.Message);
      }

      LoggingController.LogSubmission(mediaObjectId, frame);
    }

    /// <summary>
    /// Submit segment without further submission specification.
    /// </summary>
    /// <param name="segment">Segment to be submitted.</param>
    public static async void QuickSubmitSegment(SegmentData segment)
    {
      var mediaObjectId = await segment.GetObjectId();
      var mediaObject = ObjectRegistry.GetObject(mediaObjectId);
      var mediaType = await mediaObject.GetMediaType();

      switch (mediaType)
      {
        case MediaObjectDescriptor.MediatypeEnum.VIDEO:
          var startFrame = await segment.GetStart();
          var endFrame = await segment.GetEnd();
          var frame = (startFrame + endFrame) / 2;
          SubmitResult(mediaObjectId, frame);
          break;
        case MediaObjectDescriptor.MediatypeEnum.IMAGESEQUENCE:
          SubmitResult(segment.Id);
          break;
        default:
          SubmitResult(mediaObjectId);
          break;
      }
    }

    /// <summary>
    /// Logs results to the connected DRES instance.
    /// </summary>
    /// <param name="sortType">The sorting of the results display.</param>
    /// <param name="results">The results as list of scored segments.</param>
    /// <param name="queryEvents">The query that lead to these results represented as enumerable of query events.</param>
    /// <param name="timestamp">Timestamp of result log.</param>
    /// <param name="assumeFullyFetched">Skips trying to batch fetch segment data if true.</param>
    private static async void LogResults(string sortType,
      IReadOnlyCollection<(ScoredSegment segment, int rank)> results, IEnumerable<QueryEvent> queryEvents,
      long timestamp, bool assumeFullyFetched = false)
    {
      if (!assumeFullyFetched)
      {
        await SegmentRegistry.BatchFetchSegmentData(results.Select(pair => pair.segment.segment));
      }

      var queryResults = await Task.WhenAll(results.Select(async pair =>
      {
        var segment = pair.segment.segment;
        var objectId = await segment.GetObjectId();
        objectId = RemovePrefix(objectId);
        var sequenceNumber = await segment.GetSequenceNumber();
        var frame = await segment.GetStart();

        return new QueryResult(objectId, sequenceNumber, frame, pair.segment.score, pair.rank);
      }));

      var queryResultsList = queryResults.ToList();
      var queryEventsList = queryEvents.ToList();
      try
      {
        var success = await _instance.LogResults(timestamp, sortType, "top", queryResultsList, queryEventsList);

        if (!success.Status)
        {
          NotificationController.Notify($"Could not log to Dres: {success.Description}");
        }
      }
      catch (Exception e)
      {
        NotificationController.Notify(e.Message);
      }
    }

    public static void LogResults(long timestamp, string sortType, IEnumerable<ScoredSegment> results,
      SimilarityQuery query)
    {
      var queryEvents = query.Terms.Select(term =>
      {
        // Convert term type to DRES category
        var category = TermTypeToDresCategory(term.Type);

        var type = string.Join(",", term.Categories.Select(CategoryToType));
        var value = term.Data;

        return new QueryEvent(timestamp, category, type, value);
      });

      var rankedResults = results.Select((segment, rank) => (segment, rank)).ToList();

      LogResults(sortType, rankedResults, queryEvents, timestamp);
    }

    public static void LogResults(long timestamp, string sortType, IEnumerable<ScoredSegment> results,
      StagedSimilarityQuery query)
    {
      var queryEvents = query.Stages.SelectMany((stage, si) => stage.Terms.Select(term =>
      {
        // Convert term type to DRES category
        var category = TermTypeToDresCategory(term.Type);

        var type = string.Join(",", term.Categories.Select(CategoryToType));
        var value = term.Data;

        // Also provide stage index in type
        return new QueryEvent(timestamp, category, $"{si}:{type}", value);
      }));

      var rankedResults = results.Select((segment, rank) => (segment, rank)).ToList();

      LogResults(sortType, rankedResults, queryEvents, timestamp);
    }

    public static void LogResults(long timestamp, string sortType, IEnumerable<TemporalObject> results,
      TemporalQuery query)
    {
      var queryEvents = query.Queries.SelectMany(
        (temporal, ti) => temporal.Stages.SelectMany(
          (stage, si) => stage.Terms.Select(term =>
          {
            // Convert term type to DRES category
            var category = TermTypeToDresCategory(term.Type);

            var type = string.Join(",", term.Categories.Select(CategoryToType));
            var value = term.Data;

            // Also provide temporal and stage indexes in type
            return new QueryEvent(timestamp, category, $"{ti}:{si}:{type}", value);
          })
        )
      );

      var resultsList = results.SelectMany(
        (to, rank) => to.Segments.Select(
          sId => (new ScoredSegment(SegmentRegistry.GetSegment(sId), to.Score), rank)
        )
      ).ToList();

      LogResults(sortType, resultsList, queryEvents, timestamp);
    }

    private static async void LogInteraction()
    {
      if (InteractionEvents.Count <= 0) return;

      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      // Submit to DRES
      try
      {
        var success = await _instance.LogQueryEvents(timestamp, InteractionEvents);

        if (!success.Status)
        {
          NotificationController.NotifyError($"Could not log interactions to Dres: {success.Description}");
        }
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging interaction: {e.Message}", e);
      }

      InteractionEvents.Clear();
    }

    public static void LogInteraction(long timestamp, string type, string value,
      QueryEvent.CategoryEnum category = QueryEvent.CategoryEnum.BROWSING)
    {
      if (!ConfigManager.Config.dresEnabled) return;

      var queryEvent = new QueryEvent(timestamp, category, type, value);
      InteractionEvents.Add(queryEvent);
    }

    /// <summary>
    /// Converts a Cineast API compliant query term category string to a Dres API compliant type string for query event
    /// logging.
    /// </summary>
    /// <param name="category">Cineast API category string.</param>
    /// <returns>Dres API type string.</returns>
    private static string CategoryToType(string category)
    {
      return category switch
      {
        "ocr" => "OCR",
        "asr" => "ASR",
        "scenecaption" => "caption",
        "visualtextcoembedding" => "jointEmbedding",
        CategoryMappings.TAGS_CATEGORY => "concept",
        CategoryMappings.GLOBAL_COLOR_CATEGORY => "globalFeatures",
        CategoryMappings.EDGE_CATEGORY => "localFeatures",
        _ => category
      };
    }

    private static QueryEvent.CategoryEnum TermTypeToDresCategory(QueryTerm.TypeEnum? type)
    {
      return type switch
      {
        QueryTerm.TypeEnum.IMAGE => QueryEvent.CategoryEnum.IMAGE,
        QueryTerm.TypeEnum.AUDIO => QueryEvent.CategoryEnum.OTHER,
        QueryTerm.TypeEnum.MODEL3D => QueryEvent.CategoryEnum.OTHER,
        QueryTerm.TypeEnum.LOCATION => QueryEvent.CategoryEnum.OTHER,
        QueryTerm.TypeEnum.TIME => QueryEvent.CategoryEnum.OTHER,
        QueryTerm.TypeEnum.TEXT => QueryEvent.CategoryEnum.TEXT,
        QueryTerm.TypeEnum.TAG => QueryEvent.CategoryEnum.TEXT,
        QueryTerm.TypeEnum.SEMANTIC => QueryEvent.CategoryEnum.SKETCH,
        QueryTerm.TypeEnum.ID => QueryEvent.CategoryEnum.OTHER,
        QueryTerm.TypeEnum.BOOLEAN => QueryEvent.CategoryEnum.FILTER,
        _ => QueryEvent.CategoryEnum.OTHER
      };
    }

    /// <summary>
    /// Removes the configured prefix length from the given segment or object ID.
    /// </summary>
    private static string RemovePrefix(string id)
    {
      var prefixLength = ConfigManager.Config.submissionIdPrefixLength;
      return prefixLength > 0 ? id[prefixLength..] : id;
    }
  }
}
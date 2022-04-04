using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using Org.Vitrivr.DresApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Config;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using Vitrivr.UnityInterface.DresApi;
using VitrivrVR.Config;
using VitrivrVR.Notification;

namespace VitrivrVR.Submission
{
  public class DresClientManager : MonoBehaviour
  {
    public static DresClient Instance;

    private static readonly List<QueryEvent> InteractionEvents = new List<QueryEvent>();
    private static float _interactionEventTimer;

    private static string _interactionLogPath;
    private static string _resultsLogPath;
    private static string _submissionLogPath;

    private async void Start()
    {
      if (!ConfigManager.Config.dresEnabled) return;

      if (ConfigManager.Config.allowInvalidCertificate)
      {
        ServicePointManager.ServerCertificateValidationCallback +=
          (sender, certificate, chain, sslPolicyErrors) => true;
      }

      Instance = new DresClient();
      await Instance.Login();
      var logDir = ConfigManager.Config.logFileLocation;
      var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      var username = Instance.UserDetails.Username;
      var session = Instance.UserDetails.SessionId;
      _interactionLogPath = Path.Combine(logDir, $"{startTime}_{username}_{session}_interaction.txt");
      _resultsLogPath = Path.Combine(logDir, $"{startTime}_{username}_{session}_results.txt");
      _submissionLogPath = Path.Combine(logDir, $"{startTime}_{username}_{session}_submission.txt");
      NotificationController.Notify($"Dres connected: {username}");

      if (ConfigManager.Config.writeLogsToFile)
      {
        Directory.CreateDirectory(ConfigManager.Config.logFileLocation);
      }
    }

    private void Update()
    {
      if (!ConfigManager.Config.dresEnabled) return;

      //Update timer
      _interactionEventTimer += Time.deltaTime;
      if (_interactionEventTimer > ConfigManager.Config.interactionLogSubmissionInterval)
      {
        // Reset timer
        _interactionEventTimer %= ConfigManager.Config.interactionLogSubmissionInterval;
        LogInteraction();
      }
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
        var result = await Instance.SubmitResult(mediaObjectId, frame);
        NotificationController.Notify($"Submission: {result.Submission}");
      }
      catch (Exception e)
      {
        NotificationController.Notify(e.Message);
      }

      if (ConfigManager.Config.writeLogsToFile)
      {
        LogSubmissionToFile(mediaObjectId, frame);
      }
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

    private static async void LogSubmissionToFile(string mediaObjectId, int? frame)
    {
      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      try
      {
        using var file = new StreamWriter(_submissionLogPath, true);
        var row = $"{timestamp},{mediaObjectId}";
        if (frame != null)
          row += $",{frame}";
        await file.WriteLineAsync(row);
      }
      catch (Exception e)
      {
        NotificationController.Notify($"Error logging to file: {e.Message}");
      }
    }

    /// <summary>
    /// Logs results to the connected Dres instance.
    /// </summary>
    /// <param name="sortType">The sorting of the results display.</param>
    /// <param name="results">The results as list of scored segments.</param>
    /// <param name="query">The query that lead to these results.</param>
    /// <param name="assumeFullyFetched">Skips trying to batch fetch segment data if true.</param>
    public static async void LogResults(string sortType, List<ScoredSegment> results, SimilarityQuery query,
      bool assumeFullyFetched = false)
    {
      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (!assumeFullyFetched)
      {
        await SegmentRegistry.BatchFetchSegmentData(results.Select(result => result.segment));
      }

      var queryResults = await Task.WhenAll(results.Select(async (result, i) =>
      {
        var segment = result.segment;
        var objectId = await segment.GetObjectId();
        objectId = RemovePrefix(objectId);
        var sequenceNumber = await segment.GetSequenceNumber();
        var frame = await segment.GetStart();

        return new QueryResult(objectId, sequenceNumber, frame, result.score, i);
      }));

      var queryEvents = query.Terms.Select(term =>
      {
        // Convert term type to Dres category
        var category = term.Type switch
        {
          QueryTerm.TypeEnum.IMAGE => QueryEvent.CategoryEnum.IMAGE,
          QueryTerm.TypeEnum.AUDIO => QueryEvent.CategoryEnum.OTHER,
          QueryTerm.TypeEnum.MOTION => QueryEvent.CategoryEnum.OTHER,
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

        var type = string.Join(",", term.Categories.Select(CategoryToType));
        var value = term.Data;

        return new QueryEvent(timestamp, category, type, value);
      });

      var queryResultsList = queryResults.ToList();
      var queryEventsList = queryEvents.ToList();
      try
      {
        var success = await Instance.LogResults(timestamp, sortType, "top", queryResultsList, queryEventsList);

        if (!success.Status)
        {
          NotificationController.Notify($"Could not log to Dres: {success.Description}");
        }
      }
      catch (Exception e)
      {
        NotificationController.Notify(e.Message);
      }

      if (ConfigManager.Config.writeLogsToFile)
      {
        try
        {
          using var file = new StreamWriter(_resultsLogPath, true);
          var jsonResults = string.Join(",", queryResultsList.Select(q => q.ToJson().Replace("\n", "")));
          var jsonEvents = string.Join(",", queryEventsList.Select(q => q.ToJson().Replace("\n", "")));
          var resultLog = $"{timestamp},{sortType},top,[{jsonResults}],[{jsonEvents}]";
          await file.WriteLineAsync(resultLog);
        }
        catch (Exception e)
        {
          NotificationController.NotifyError($"Error logging to file: {e.Message}", e);
        }
      }
    }

    private static async void LogInteraction()
    {
      if (InteractionEvents.Count <= 0) return;

      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      // Submit to DRES
      try
      {
        var success = await Instance.LogQueryEvents(timestamp, InteractionEvents);

        if (!success.Status)
        {
          NotificationController.NotifyError($"Could not log interactions to Dres: {success.Description}");
        }
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging interaction: {e.Message}", e);
      }

      var events = InteractionEvents.ToArray();
      InteractionEvents.Clear();

      // Write to file
      if (ConfigManager.Config.writeLogsToFile)
      {
        try
        {
          using var file = new StreamWriter(_interactionLogPath, true);
          foreach (var interactionEvent in events)
          {
            await file.WriteLineAsync(interactionEvent.ToJson().Replace("\n", ""));
          }
        }
        catch (Exception e)
        {
          NotificationController.NotifyError($"Error logging to file: {e.Message}", e);
        }
      }
    }

    public static void LogInteraction(string type, string value,
      QueryEvent.CategoryEnum category = QueryEvent.CategoryEnum.BROWSING)
    {
      if (!ConfigManager.Config.dresEnabled) return;

      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

    /// <summary>
    /// Removes the configured prefix length from the given segment or object ID.
    /// </summary>
    private static string RemovePrefix(string id)
    {
      var prefixLength = ConfigManager.Config.submissionIdPrefixLength;
      return prefixLength > 0 ? id.Substring(prefixLength) : id;
    }
  }
}
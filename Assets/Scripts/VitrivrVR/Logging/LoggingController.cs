using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Dev.Dres.ClientApi.Model;
using Newtonsoft.Json;
using Org.Vitrivr.CineastApi.Model;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Notification;
using VitrivrVR.Submission;

namespace VitrivrVR.Logging
{
  /// <summary>
  /// Static class for all interaction and usage logs.
  ///
  /// The goal of this class is to separate the log sources from different kinds of loggers, such as file and
  /// competition loggers.
  /// </summary>
  public static class LoggingController
  {
    private static readonly string StartDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    private static readonly string LogDir = Path.Combine(ConfigManager.Config.logFileLocation, StartDate);
    private static readonly string ResultLogLocation = Path.Combine(LogDir, "results.txt");
    private static readonly string SubmissionLogLocation = Path.Combine(LogDir, "submission.txt");
    private static readonly string InteractionLogLocation = Path.Combine(LogDir, "interactions.txt");

    private static readonly SemaphoreSlim SubmissionLogLock = new(1, 1);
    private static readonly SemaphoreSlim ResultLogLock = new(1, 1);
    private static readonly SemaphoreSlim InteractionLogLock = new(1, 1);

    private static long CurrentTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Logs ranked results lists for similarity and staged similarity queries.
    /// </summary>
    /// <param name="sortOrder">The order in which the ranked results are displayed (e.g. by segment or object).</param>
    /// <param name="results">List of ranked results.</param>
    /// <param name="queryResponse">Query response containing the source query.</param>
    public static void LogQueryResults(string sortOrder, List<ScoredSegment> results, QueryResponse queryResponse)
    {
      var timestamp = CurrentTimestamp;

      // Log to DRES
      if (ConfigManager.Config.dresEnabled)
      {
        // Similarity query
        if (queryResponse.Query != null)
        {
          DresClientManager.LogResults(timestamp, sortOrder, results, queryResponse.Query);
        }

        // Staged query
        if (queryResponse.StagedQuery != null)
        {
          DresClientManager.LogResults(timestamp, sortOrder, results, queryResponse.StagedQuery);
        }
      }

      // Log to file
      if (ConfigManager.Config.writeLogsToFile)
      {
        LogQueryResultsToFile(timestamp, sortOrder, results, queryResponse);
      }
    }

    /// <summary>
    /// Logs ranked results lists for temporal similarity queries.
    /// </summary>
    public static void LogQueryResults(string sortOrder, List<TemporalObject> results,
      TemporalQueryResponse queryResponse)
    {
      var timestamp = CurrentTimestamp;

      // Log to DRES
      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogResults(timestamp, sortOrder, results, queryResponse.Query);
      }

      // Log to file
      if (ConfigManager.Config.writeLogsToFile)
      {
        LogQueryResultsToFile(timestamp, sortOrder, results, queryResponse);
      }
    }

    public static void LogSubmission(string mediaObjectId, int? frame)
    {
      var timestamp = CurrentTimestamp;
      // Log to file
      if (ConfigManager.Config.writeLogsToFile)
      {
        LogSubmissionToFile(timestamp, mediaObjectId, frame);
      }
    }

    public static void LogInteraction(string type, string value, QueryEvent.CategoryEnum category)
    {
      var timestamp = CurrentTimestamp;

      // Log to DRES
      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogInteraction(timestamp, type, value, category);
      }

      // Log to file
      if (ConfigManager.Config.writeLogsToFile)
      {
        LogInteractionToFile(timestamp, type, value, category.ToString());
      }
    }

    #region FileLogger

    private static void EnsureDirectoryExists()
    {
      Directory.CreateDirectory(LogDir);
    }

    private static async void LogQueryResultsToFile(long timestamp, string sortOrder,
      IEnumerable<ScoredSegment> results,
      QueryResponse queryResponse)
    {
      EnsureDirectoryExists();
      await ResultLogLock.WaitAsync();
      try
      {
        await using var file = new StreamWriter(ResultLogLocation, true);
        var serializableResults = results.Select(segment => new Dictionary<string, string>
        {
          { "id", segment.segment.Id },
          { "score", segment.score.ToString(CultureInfo.InvariantCulture) }
        });
        var jsonResults = JsonConvert.SerializeObject(serializableResults, Formatting.None);
        var jsonQuery = QueryToJson(queryResponse);

        var resultLog =
          $"{{\"timestamp\":{timestamp},\"sortOrder\":\"{sortOrder}\",\"query\":{jsonQuery},\"results\":{jsonResults}}}";
        await file.WriteLineAsync(resultLog);
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging to file: {e.Message}", e);
      }
      finally
      {
        ResultLogLock.Release();
      }
    }

    private static async void LogQueryResultsToFile(long timestamp, string sortOrder, List<TemporalObject> results,
      TemporalQueryResponse queryResponse)
    {
      EnsureDirectoryExists();
      await ResultLogLock.WaitAsync();
      try
      {
        await using var file = new StreamWriter(ResultLogLocation, true);
        var jsonResults = JsonConvert.SerializeObject(results, Formatting.None);
        var jsonQuery = JsonConvert.SerializeObject(queryResponse.Query, Formatting.None);

        var resultLog =
          $"{{\"timestamp\":{timestamp},\"sortOrder\":\"{sortOrder}\",\"query\":{jsonQuery},\"results\":{jsonResults}}}";
        await file.WriteLineAsync(resultLog);
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging to file: {e.Message}", e);
      }
      finally
      {
        ResultLogLock.Release();
      }
    }

    private static string QueryToJson(QueryResponse queryResponse)
    {
      if (queryResponse.Query != null)
      {
        return JsonConvert.SerializeObject(queryResponse.Query, Formatting.None);
      }

      if (queryResponse.StagedQuery != null)
      {
        return JsonConvert.SerializeObject(queryResponse.StagedQuery, Formatting.None);
      }

      throw new Exception("Query response contains neither similarity nor staged similarity query.");
    }

    private static async void LogSubmissionToFile(long timestamp, string mediaObjectId, int? frame)
    {
      EnsureDirectoryExists();
      await SubmissionLogLock.WaitAsync();
      try
      {
        await using var file = new StreamWriter(SubmissionLogLocation, true);
        var dict = new Dictionary<string, string>
        {
          { "timestamp", timestamp.ToString() },
          { "mediaObjectId", mediaObjectId }
        };
        if (frame.HasValue)
          dict["frame"] = frame.ToString();

        var json = JsonConvert.SerializeObject(dict, Formatting.None);

        await file.WriteLineAsync(json);
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging to file: {e.Message}");
      }
      finally
      {
        SubmissionLogLock.Release();
      }
    }

    private static async void LogInteractionToFile(long timestamp, string type, string value, string category)
    {
      EnsureDirectoryExists();
      await InteractionLogLock.WaitAsync();
      try
      {
        await using var file = new StreamWriter(InteractionLogLocation, true);
        var dict = new Dictionary<string, string>
        {
          { "timestamp", timestamp.ToString() },
          { "type", type },
          { "value", value },
          { "category", category }
        };

        var json = JsonConvert.SerializeObject(dict, Formatting.None);

        await file.WriteLineAsync(json);
      }
      catch (Exception e)
      {
        NotificationController.NotifyError($"Error logging to file: {e.Message}");
      }
      finally
      {
        InteractionLogLock.Release();
      }
    }

    #endregion
  }
}
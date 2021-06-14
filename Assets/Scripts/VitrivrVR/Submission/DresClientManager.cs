using System;
using System.Collections.Generic;
using System.Linq;
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
    public static DresClient instance;

    private async void Start()
    {
      if (ConfigManager.Config.dresEnabled)
      {
        instance = new DresClient();
        await instance.Login();
        NotificationController.Notify($"Dres connected: {instance.UserDetails.Username}");
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
        var sequenceNumber = await segment.GetSequenceNumber();
        var frame = await segment.GetStart();

        return new QueryResult(objectId, sequenceNumber, frame, result.score, i);
      }));

      var queryEvents = query.Containers.First().Terms.Select(term =>
      {
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

      var success = await instance.LogResults(timestamp, sortType, "top", queryResults.ToList(), queryEvents.ToList());

      if (!success.Status)
      {
        NotificationController.Notify($"Could not log to Dres: {success.Description}");
      }
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
  }
}
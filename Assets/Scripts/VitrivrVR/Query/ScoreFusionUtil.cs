using System.Collections.Generic;
using System.Linq;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Query
{
  /// <summary>
  /// Class to control query score fusion.
  /// </summary>
  public static class ScoreFusionUtil
  {
    public static List<ScoredSegment> FuseScores(QueryResponse response)
    {
      return Config.ConfigManager.Config.scoreFusion switch
      {
        Config.VitrivrVrConfig.ScoreFusion.MeanFusion => response.GetMeanFusionResults(),
        Config.VitrivrVrConfig.ScoreFusion.SimpleFusion => SimpleFusion(response),
        _ => throw new System.Exception($"Unknown score fusion method: {Config.ConfigManager.Config.scoreFusion}")
      };
    }

    /// <summary>
    /// Fusion method used by the SimpleQueryTermManager.
    /// Spatial results are used to filter the MLClip results if available.
    /// To determine the spatial filter cutoff, the half-similarity distance is used.
    /// </summary>
    private static List<ScoredSegment> SimpleFusion(QueryResponse response)
    {
      if (response.Results.Count == 1)
      {
        return response.Results.Values.First();
      }

      // If the spatial results exist, we only use the IDs from the spatial results that have a similarity greater than 0.5
      var ids = response.Results.TryGetValue("spatialdistance", out var result)
        ? result.Where(segment => segment.score > 0.5).Select(segment => segment.segment.Id).ToHashSet()
        : null;

      var scores = response.Results.GetValueOrDefault("mlclip");

      return (ids, scores) switch
      {
        (null, null) => response.GetMeanFusionResults(),
        (null, _) => scores,
        (_, null) => response.Results["spatialdistance"].Where(segment => ids.Contains(segment.segment.Id))
          .Select(segment => new ScoredSegment(segment.segment, 1)).ToList(),
        _ => scores.Where(segment => ids.Contains(segment.segment.Id)).ToList()
      };
    }
  }
}
using System;
using System.Collections.Generic;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Config;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;

namespace VitrivrVR.Config
{
  [Serializable]
  public class VitrivrVrConfig
  {
    [Serializable]
    public class ConfigColor
    {
      public float r, g, b;

      public ConfigColor(float r, float g, float b)
      {
        this.r = r;
        this.g = g;
        this.b = b;
      }

      public Color ToColor() => new Color(r, g, b);
    }

    /// <summary>
    /// The maximum number of results to accept from a single query.
    /// </summary>
    public int maxResults;

    /// <summary>
    /// The maximum number of query results to prefetch.
    /// </summary>
    public int maxPrefetch;

    /// <summary>
    /// The maximum number of items to display at once.
    /// </summary>
    public int maxDisplay;

    /// <summary>
    /// The color to indicate the minimum score for returned results.
    /// </summary>
    public ConfigColor dissimilarityColor;

    /// <summary>
    /// The color to indicate the maximum score for returned results.
    /// </summary>
    public ConfigColor similarityColor;

    /// <summary>
    /// Sets the debug output for dictation and speech-to-text.
    /// </summary>
    public bool dictationDebugOutput;

    /// <summary>
    /// The default categories for image query-by-example.
    /// </summary>
    public List<string> defaultImageCategories;

    /// <summary>
    /// The default volume [0, 1] to use for audio and video.
    /// </summary>
    public float defaultMediaVolume;

    private VitrivrVrConfig()
    {
      maxResults = 10000;
      maxPrefetch = 1000;
      maxDisplay = 100;
      dissimilarityColor = new ConfigColor(1, 0, 0);
      similarityColor = new ConfigColor(0, 1, 0);
      dictationDebugOutput = false;
      var mapping = CineastConfigManager.Instance.Config.categoryMappings.mapping;
      defaultImageCategories = new List<string>
      {
        mapping[CategoryMappings.GLOBAL_COLOR_CATEGORY],
        mapping[CategoryMappings.EDGE_CATEGORY]
      };
      defaultMediaVolume = .5f;
    }

    public static VitrivrVrConfig GetDefault()
    {
      return new VitrivrVrConfig();
    }
  }
}
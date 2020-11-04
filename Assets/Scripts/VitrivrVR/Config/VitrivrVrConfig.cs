using System;
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

    public ConfigColor dissimilarityColor;
    public ConfigColor similarityColor;

    private VitrivrVrConfig()
    {
      maxResults = 10000;
      maxPrefetch = 1000;
      maxDisplay = 100;
      dissimilarityColor = new ConfigColor(1, 0, 0);
      similarityColor = new ConfigColor(0, 1, 0);
    }

    public static VitrivrVrConfig GetDefault()
    {
      return new VitrivrVrConfig();
    }
  }
}
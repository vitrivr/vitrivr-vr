using System;

namespace VitrivrVR.Config
{
  [Serializable]
  public class VitrivrVrConfig
  {
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

    private VitrivrVrConfig()
    {
      maxResults = 10000;
      maxPrefetch = 1000;
      maxDisplay = 100;
    }

    public static VitrivrVrConfig GetDefault()
    {
      return new VitrivrVrConfig();
    }
  }
}
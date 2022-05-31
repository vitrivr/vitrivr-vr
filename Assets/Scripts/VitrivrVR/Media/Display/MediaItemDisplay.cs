using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Abstract class for media display items representing a media segment.
  /// </summary>
  public abstract class MediaItemDisplay : MonoBehaviour
  {
    public abstract ScoredSegment ScoredSegment { get; }

    /// <summary>
    /// Initializes this display with the given segment data.
    /// </summary>
    /// <param name="segment">Segment to initialize this display with</param>
    public abstract void Initialize(ScoredSegment segment);
  }
}
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Can be used to pass along media item display parameters.
  /// </summary>
  public class MetaMediaItemDisplay : MediaItemDisplay
  {
    public MediaItemDisplay mediaItemDisplay;

    public override ScoredSegment ScoredSegment => mediaItemDisplay.ScoredSegment;

    public override void Initialize(ScoredSegment segment)
    {
      mediaItemDisplay.Initialize(segment);
    }
  }
}
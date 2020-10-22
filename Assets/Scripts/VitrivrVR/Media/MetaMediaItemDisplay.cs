using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Can be used to pass along media item display parameters.
  /// </summary>
  public class MetaMediaItemDisplay : MediaItemDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    
    public override Task Initialize(SegmentData segment)
    {
      return mediaItemDisplay.Initialize(segment);
    }
  }
}
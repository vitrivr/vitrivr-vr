using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Displays a <see cref="TemporalObject"/>.
  /// </summary>
  public class TemporalMediaItemDisplay : MonoBehaviour
  {
    public MediaItemDisplay mediaItemDisplay;

    private const float DisplayDistance = 0.3f;

    public void Initialize(TemporalObject temporalObject)
    {
      var rotation = transform.rotation;

      foreach (var (segmentId, i) in temporalObject.Segments.Select((sid, i) => (sid, i)))
      {
        var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, rotation, transform);

        var it = itemDisplay.transform;
        it.localPosition = Vector3.forward * (DisplayDistance * i);

        var segment = SegmentRegistry.GetSegment(segmentId);
        var scoredSegment = new ScoredSegment(segment, temporalObject.Score);

        itemDisplay.Initialize(scoredSegment);
      }
    }
  }
}
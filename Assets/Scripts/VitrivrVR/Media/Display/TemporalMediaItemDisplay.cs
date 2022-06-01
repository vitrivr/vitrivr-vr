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
    public Transform displayParent;

    private const float DisplayDistance = 0.3f;

    public void Initialize(TemporalObject temporalObject)
    {
      var rotation = transform.rotation;

      foreach (var (segmentId, i) in temporalObject.Segments.Select((sid, i) => (sid, i)))
      {
        var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, rotation, displayParent);

        var it = itemDisplay.transform;
        it.localPosition = Vector3.forward * (DisplayDistance * i);

        var segment = SegmentRegistry.GetSegment(segmentId);
        var scoredSegment = new ScoredSegment(segment, temporalObject.Score);

        itemDisplay.Initialize(scoredSegment);
      }

      if (!displayParent.TryGetComponent<BoxCollider>(out var boxCollider)) return;
      var size = boxCollider.size;
      var center = boxCollider.center;

      size.z = temporalObject.Segments.Count * DisplayDistance;
      center.z = (size.z - DisplayDistance) / 2;

      boxCollider.size = size;
      boxCollider.center = center;
    }
  }
}
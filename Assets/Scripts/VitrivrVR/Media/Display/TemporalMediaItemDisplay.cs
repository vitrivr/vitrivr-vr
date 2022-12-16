using System.Linq;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Displays a <see cref="TemporalResult"/>.
  /// </summary>
  public class TemporalMediaItemDisplay : MonoBehaviour
  {
    public MediaItemDisplay mediaItemDisplay;
    public Transform displayParent;

    private const float DisplayDistance = 0.3f;

    public void Initialize(TemporalResult temporalResult)
    {
      var rotation = transform.rotation;

      foreach (var (segment, i) in temporalResult.Segments.Select((sid, i) => (sid, i)))
      {
        var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, rotation, displayParent);

        var it = itemDisplay.transform;
        it.localPosition = Vector3.forward * (DisplayDistance * i);

        var scoredSegment = new ScoredSegment(segment, temporalResult.Score);

        itemDisplay.Initialize(scoredSegment);
      }

      if (!displayParent.TryGetComponent<BoxCollider>(out var boxCollider)) return;
      var size = boxCollider.size;
      var center = boxCollider.center;

      size.z = temporalResult.Segments.Count * DisplayDistance;
      center.z = (size.z - DisplayDistance) / 2;

      boxCollider.size = size;
      boxCollider.center = center;
    }
  }
}
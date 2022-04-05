using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseProjectionController : MonoBehaviour
  {
    public GameObject keyPointIndicatorPrefab;
    public KeyPointController[] keyPoints = { };

    private List<(KeyPointController point, Transform indicator)> _keyPointIndicators;

    private void Start()
    {
      var parent = transform.parent;
      _keyPointIndicators =
        keyPoints.Select(point => (point, Instantiate(keyPointIndicatorPrefab, parent).transform)).ToList();
    }

    private void Update()
    {
      var t = transform;
      foreach (var (point, indicator) in _keyPointIndicators)
      {
        var position = PointToCanvasSpace(point.transform.position);
        indicator.position = t.TransformPoint(position);
        indicator.gameObject.SetActive(PointWithinBounds(position));
      }
    }

    /// <summary>
    /// Checks if the given point is within projection surface bounds.
    /// 
    /// Only checks x and y coordinates and expects point to already be in local space.
    /// </summary>
    /// <param name="point">Point in local space.</param>
    /// <returns>True if point is within bounds of the projection surface.</returns>
    private static bool PointWithinBounds(Vector3 point)
    {
      const float xBound = .5f;
      const float yBound = .5f;
      return point.x is > -xBound and < xBound && point.y is > -yBound and < yBound;
    }

    private Vector3 PointToCanvasSpace(Vector3 point)
    {
      var canvasPoint = transform.InverseTransformPoint(point);
      canvasPoint.z = -0.001f; // Not set to 0 to prevent z-fighting

      return canvasPoint;
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.green;

      var t = transform;
      foreach (var point in keyPoints)
      {
        if (point == null) continue;

        var position = t.InverseTransformPoint(point.transform.position);
        position.z = 0;
        if (!PointWithinBounds(position)) continue;

        Gizmos.DrawSphere(t.TransformPoint(position), .002f);
      }
    }
  }
}
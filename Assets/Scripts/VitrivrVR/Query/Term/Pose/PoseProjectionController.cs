using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseProjectionController : MonoBehaviour
  {
    public GameObject keyPointIndicatorPrefab;
    public KeyPointController[] keyPoints = { };
    public Material lineMaterial;
    public float lineWidth = 0.001f;

    private List<(KeyPointController point, Transform indicator)> _keyPointIndicators;
    private List<(Transform indicator0, Transform indicator1, LineRenderer line)> _keyPointConnections;

    private void Start()
    {
      var parent = transform.parent;
      _keyPointIndicators =
        keyPoints.Select(point => (point, Instantiate(keyPointIndicatorPrefab, parent).transform)).ToList();

      var indicatorDictionary = _keyPointIndicators.ToDictionary(pair => pair.point, pair => pair.indicator);

      _keyPointConnections = _keyPointIndicators.SelectMany(pair =>
      {
        var (point, indicator) = pair;
        return point.connectedPoints.Select(other =>
        {
          var otherIndicator = indicatorDictionary[other];
          var go = new GameObject($"Projection {point.name} - {other.name}", typeof(LineRenderer));
          go.transform.SetParent(parent);

          var line = go.GetComponent<LineRenderer>();
          line.material = lineMaterial;
          line.widthMultiplier = lineWidth;
          line.numCapVertices = 2;
          line.numCornerVertices = 2;

          line.SetPositions(new[] { indicator.position, otherIndicator.position });

          return (indicator, otherIndicator, line);
        });
      }).ToList();
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

      foreach (var (indicator0, indicator1, line) in _keyPointConnections)
      {
        var forward = new Vector3(0, 0, indicator0.localPosition.z);
        line.SetPosition(0, indicator0.position - 1.01f * forward);
        line.SetPosition(1, indicator1.position - 1.01f * forward);

        line.gameObject.SetActive(indicator0.gameObject.activeSelf && indicator1.gameObject.activeSelf);
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
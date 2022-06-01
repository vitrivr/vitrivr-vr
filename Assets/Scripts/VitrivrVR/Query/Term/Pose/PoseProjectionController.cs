using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseProjectionController : MonoBehaviour
  {
    public GameObject keyPointIndicatorPrefab;
    public Material lineMaterial;
    public float lineWidth = 0.001f;

    private const float FOV = 90;

    /// <summary>
    /// Dictionary mapping pose skeleton controllers to the lists of their projection canvas indicators as well as the
    /// canvas lines connecting them.
    /// </summary>
    private readonly Dictionary<PoseSkeletonController, (
      List<(KeyPointController point, Transform indicator)> indicators,
      List<(Transform indicator0, Transform indicator1, LineRenderer line
        )> connections)> _skeletons = new();

    private static readonly float NearPlane = .5f / Mathf.Tan(Mathf.Deg2Rad * (FOV / 2));

    private Matrix4x4 _projectionMatrix = GetProjectionMatrix();

    public List<List<(Vector2 point, float weight)>> GetPoints()
    {
      return _skeletons.Keys
        .Select(skeleton => skeleton.KeyPoints
          .Select(point =>
          {
            var coordinates = PointToCanvasSpace2(point.transform.position);
            var weight = point.Active && PointWithinBounds(coordinates) ? 1f : 0f;

            return (coordinates, weight);
          }).ToList()).ToList();
    }

    public void AddPoseSkeleton(PoseSkeletonController skeleton)
    {
      var indicators = CreateIndicators(skeleton);
      _skeletons.Add(skeleton, indicators);
      skeleton.onClose = () => RemovePoseSkeleton(skeleton);
    }

    public void RemovePoseSkeleton(PoseSkeletonController skeleton)
    {
      var (indicators, connections) = _skeletons[skeleton];
      foreach (var (_, _, line) in connections)
      {
        Destroy(line.gameObject);
      }

      foreach (var (_, indicator) in indicators)
      {
        Destroy(indicator.gameObject);
      }

      _skeletons.Remove(skeleton);
    }

    private static Matrix4x4 GetProjectionMatrix()
    {
      var matrix = Matrix4x4.Perspective(FOV, 1, NearPlane, 100);
      matrix.SetColumn(2, matrix.GetColumn(2) * -1);

      return matrix;
    }

    private (List<(KeyPointController, Transform)>, List<(Transform, Transform, LineRenderer)>) CreateIndicators(
      PoseSkeletonController skeleton)
    {
      // Create canvas indicators
      var parent = transform.parent;
      List<(KeyPointController point, Transform indicator)> keyPointIndicators =
        skeleton.KeyPoints.Select(point => (point, Instantiate(keyPointIndicatorPrefab, parent).transform)).ToList();

      // Build temporary dictionary to allow quicker line connection
      var indicatorDictionary = keyPointIndicators.ToDictionary(pair => pair.point, pair => pair.indicator);

      // Create canvas line connections
      var keyPointConnections = keyPointIndicators.SelectMany(pair =>
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

          line.SetPositions(new[] {indicator.position, otherIndicator.position});

          return (indicator, otherIndicator, line);
        });
      }).ToList();

      return (keyPointIndicators, keyPointConnections);
    }

    private void Update()
    {
      var t = transform;
      foreach (var (keyPointIndicators, keyPointConnections) in _skeletons.Values)
      {
        // Update indicators
        foreach (var (point, indicator) in keyPointIndicators)
        {
          var position = PointToCanvasCoordinates(point.transform.position);
          indicator.position = t.TransformPoint(new Vector3(position.x * 0.5f, position.y * 0.5f, -0.01f));
          indicator.gameObject.SetActive(PointWithinBounds(position));
        }

        // Update connections
        foreach (var (indicator0, indicator1, line) in keyPointConnections)
        {
          line.SetPosition(0, indicator0.position);
          line.SetPosition(1, indicator1.position);

          line.gameObject.SetActive(indicator0.gameObject.activeSelf && indicator1.gameObject.activeSelf);
        }
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
      const float xBound = 1f;
      const float yBound = 1f;
      return point.z >= -1 && point.x is > -xBound and < xBound && point.y is > -yBound and < yBound;
    }

    private Vector3 PointToCanvasCoordinates(Vector3 point)
    {
      var canvasPoint = transform.InverseTransformPoint(point) + Vector3.forward * NearPlane;

      return _projectionMatrix.MultiplyPoint(canvasPoint);
    }

    private Vector2 PointToCanvasSpace2(Vector3 point)
    {
      var canvasPoint = transform.InverseTransformPoint(point);

      return canvasPoint;
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class KeyPointController : MonoBehaviour
  {
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.blue;

    /// <summary>
    /// Connected points are only those to which lines should be drawn.
    /// Two neighboring points do not need to both have each other as connected.
    /// </summary>
    public KeyPointController[] connectedPoints = { };

    public bool Active { get; private set; } = true;

    private List<(KeyPointController point, LineRenderer line)> _lines;
    private Material _material;

    public void OnInteraction(Transform _, bool start)
    {
      if (start)
      {
        ToggleActive();
      }
    }

    private void ToggleActive()
    {
      Active = !Active;
      _material.color = Active ? activeColor : inactiveColor;
    }

    private void Start()
    {
      var position = transform.position;
      _lines = connectedPoints.Select(point =>
      {
        var go = new GameObject($"{gameObject.name} - {point.name}", typeof(LineRenderer));
        go.transform.SetParent(transform);

        var line = go.GetComponent<LineRenderer>();
        line.material = lineMaterial;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        line.SetPositions(new[] { position, point.transform.position });

        return (point, line);
      }).ToList();

      _material = GetComponent<Renderer>().material;
    }

    private void Update()
    {
      var position = transform.position;
      foreach (var (point, line) in _lines)
      {
        line.SetPosition(0, position);
        line.SetPosition(1, point.transform.position);
      }
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.red;

      var position = transform.position;
      foreach (var point in connectedPoints)
      {
        if (point != null)
        {
          Gizmos.DrawLine(position, point.transform.position);
        }
      }
    }
  }
}
using UnityEngine;

namespace VitrivrVR.Util
{
  public class ConnectionLineController : MonoBehaviour
  {
    public Transform start;
    public Transform end;

    private LineRenderer _line;

    private void Awake()
    {
      _line = GetComponent<LineRenderer>();

      if (_line.positionCount != 2)
      {
        _line.SetPositions(new[] {Vector3.zero, Vector3.zero});
      }
    }

    private void OnEnable()
    {
      Update();
    }

    private void OnDisable()
    {
      _line.SetPosition(0, Vector3.zero);
      _line.SetPosition(1, Vector3.zero);
    }

    private void Update()
    {
      _line.SetPosition(0, start.position);
      _line.SetPosition(1, end.position);
    }
  }
}
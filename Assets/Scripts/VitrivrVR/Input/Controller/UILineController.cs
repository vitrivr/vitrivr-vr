using UnityEngine;

namespace VitrivrVR.Input.Controller
{
  /// <summary>
  /// Very simple pointer style UI line.
  ///
  /// Only works on canvases that can be hit via raycast (are attached to an object with a collider).
  /// </summary>
  public class UILineController : MonoBehaviour
  {
    private LineRenderer _line;
    
    private void Start()
    {
      _line = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
      if (Physics.Raycast(transform.position, transform.forward, out var hit, 3) && hit.transform.GetComponentInChildren<Canvas>() != null)
      {
        _line.SetPosition(1, hit.distance * Vector3.forward);
        _line.enabled = true;
      }
      else
      {
        _line.SetPosition(1, Vector3.zero);
        _line.enabled = false;
      }
    }
  }
}
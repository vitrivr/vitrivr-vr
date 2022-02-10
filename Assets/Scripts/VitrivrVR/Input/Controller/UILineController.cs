using UnityEngine;
using UnityEngine.InputSystem.UI;
using VitrivrVR.Util;

namespace VitrivrVR.Input.Controller
{
  /// <summary>
  /// Very simple pointer style UI line.
  ///
  /// Only works on canvases that can be hit via raycast (are attached to an object with a collider).
  /// </summary>
  public class UILineController : MonoBehaviour
  {
    private int _pointerId = -1;

    private LineRenderer _line;
    private InputSystemUIInputModule _uiInputModule;

    private void Start()
    {
      _uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
      _line = GetComponent<LineRenderer>();
      var canvasObject = transform.GetChild(0).gameObject;
      canvasObject.GetComponent<Canvas>().worldCamera = Camera.main;
      var hoverHandler = canvasObject.AddComponent<HoverHandler>();
      hoverHandler.onEnter = data =>
      {
        _pointerId = data.pointerId;
        Destroy(canvasObject);
      };
    }

    private void FixedUpdate()
    {
      if (_pointerId == -1)
      {
        return;
      }
      
      var result = _uiInputModule.GetLastRaycastResult(_pointerId);

      if (result.isValid)
      {
        _line.SetPosition(1, result.distance * Vector3.forward);
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
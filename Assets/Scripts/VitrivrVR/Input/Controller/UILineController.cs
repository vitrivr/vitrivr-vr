using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;

namespace VitrivrVR.Input.Controller
{
  /// <summary>
  /// Very simple pointer style UI line.
  /// </summary>
  public class UILineController : MonoBehaviour
  {
    public bool rightHand;

    private int _pointerId = -1;

    private LineRenderer _line;
    private InputSystemUIInputModule _uiInputModule;

    private void Start()
    {
      _uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
      _line = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
      // Poll for connected XR controllers
      if (_pointerId == -1)
      {
        if (rightHand)
        {
          if (XRController.rightHand != null)
          {
            _pointerId = XRController.rightHand.deviceId;
          }
        }
        else
        {
          if (XRController.leftHand != null)
          {
            _pointerId = XRController.leftHand.deviceId;
          }
        }

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
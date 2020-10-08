using UnityEngine;

namespace VitrivrVR.Behavior
{
  /// <summary>
  /// Very basic component for objects spinning along an axis.
  /// </summary>
  public class SpinnerController : MonoBehaviour
  {
    public float spinSpeed = -180;
    public Vector3 axis;

    private void Start()
    {
      if (axis == Vector3.zero)
      {
        axis = transform.forward;
      }
    }

    private void Update()
    {
      transform.Rotate(axis, Time.deltaTime * spinSpeed);
    }
  }
}
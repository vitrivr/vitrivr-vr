using System;
using UnityEngine;

namespace VitrivrVR.Behavior
{
  /// <summary>
  /// Very basic component for objects spinning along an axis.
  /// </summary>
  public class SpinnerController : MonoBehaviour
  {
    public float spinSpeed = -180;

    private void Update()
    {
      transform.Rotate(transform.forward, Time.deltaTime * spinSpeed, Space.World);
    }
  }
}
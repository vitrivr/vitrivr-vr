using UnityEngine;

namespace VitrivrVR.Behavior
{
  public class SpinnerController : MonoBehaviour
  {
    public float spinSpeed = -180;

    void Update()
    {
      transform.Rotate(transform.forward, Time.deltaTime * spinSpeed);
    }
  }
}
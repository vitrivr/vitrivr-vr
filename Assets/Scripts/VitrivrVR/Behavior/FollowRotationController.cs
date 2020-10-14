using UnityEngine;

namespace VitrivrVR.Behavior
{
  public class FollowRotationController : MonoBehaviour
  {
    public bool overwriteOnEnable = true;
    public Transform target;
    public float maxAngularSpeed = 360;

    private void OnEnable()
    {
      if (overwriteOnEnable)
      {
        transform.rotation = target.rotation;
      }
    }

    private void Update()
    {
      var tfm = transform;
      tfm.rotation = Quaternion.RotateTowards(tfm.rotation, target.rotation, maxAngularSpeed * Time.deltaTime);
    }
  }
}
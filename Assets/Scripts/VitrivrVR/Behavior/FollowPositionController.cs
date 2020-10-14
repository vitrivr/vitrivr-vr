using UnityEngine;

namespace VitrivrVR.Behavior
{
  public class FollowPositionController : MonoBehaviour
  {
    public bool teleportOnEnable = true;
    public Transform target;
    public float maxSpeed = 2;

    private void OnEnable()
    {
      if (teleportOnEnable)
      {
        transform.position = target.position;
      }
    }

    private void Update()
    {
      var tfm = transform;
      tfm.position = Vector3.MoveTowards(tfm.position, target.position, maxSpeed * Time.deltaTime);
    }
  }
}
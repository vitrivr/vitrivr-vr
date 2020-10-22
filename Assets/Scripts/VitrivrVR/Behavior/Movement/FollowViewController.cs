using UnityEngine;

namespace VitrivrVR.Behavior.Movement
{
  /// <summary>
  /// Behavior controller moving the transform it is attached to to a location within the specified distance and view
  /// bounds of the target.
  /// </summary>
  public class FollowViewController : MonoBehaviour
  {
    public Transform target;
    public float minimumDistance = 1;
    public float maximumDistance = 1.5f;
    public float targetFov = 90;
    public bool followHeight = true;
    public float minimumHeight;
    public bool lookAt = true;

    private float _sqrMaxDist, _sqrMinDist, _halfFov;

    private void Awake()
    {
      _sqrMinDist = minimumDistance * minimumDistance;
      _sqrMaxDist = maximumDistance * maximumDistance;
      _halfFov = targetFov / 2;
    }

    private void Update()
    {
      // Determine target position
      var tfm = transform;
      var pos = tfm.position;
      var targetPos = target.position;

      // Find distance in X,Z plane
      var xzPos = new Vector2(pos.x, pos.z);
      var xzTargetPos = new Vector2(targetPos.x, targetPos.z);
      var xzDelta = xzPos - xzTargetPos;
      var distSqr = xzDelta.sqrMagnitude;

      if (distSqr < _sqrMinDist)
      {
        xzDelta.Normalize();
        xzDelta *= minimumDistance;
      }
      else if (distSqr > _sqrMaxDist)
      {
        xzDelta.Normalize();
        xzDelta *= maximumDistance;
      }

      // Find angle between target forward and adjusted position
      var targetForward = target.forward;
      var xzTargetForward = new Vector2(targetForward.x, targetForward.z);
      var xzAngle = Vector2.SignedAngle(xzTargetForward, xzDelta);

      var delta = new Vector3(xzDelta.x, 0, xzDelta.y);

      // Rotate to within FOV
      if (xzAngle > _halfFov)
      {
        delta = Quaternion.Euler(0, xzAngle - _halfFov, 0) * delta;
      }
      else if (xzAngle < -_halfFov)
      {
        delta = Quaternion.Euler(0, xzAngle + _halfFov, 0) * delta;
      }

      // Determine height
      delta.y = followHeight ? targetPos.y : pos.y;
      delta.y = Mathf.Max(delta.y, minimumHeight);

      // Finally add target x and z positions
      delta.x += targetPos.x;
      delta.z += targetPos.z;

      // Move to target position
      transform.position = delta;

      if (lookAt)
      {
        tfm.LookAt(target);
      }
    }
  }
}
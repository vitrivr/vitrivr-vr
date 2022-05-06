using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseSkeletonSelection : MonoBehaviour
  {
    public Transform projectionSurfaceRoot;
    public PoseProjectionController projectionSurface;
    public PoseSkeletonController poseSkeletonPrefab;

    private bool _grabbed;
    private Vector3 _offset;
    private Quaternion _rotationOffset;

    private void Start()
    {
      _offset = projectionSurfaceRoot.InverseTransformPoint(transform.position);
      _rotationOffset = Quaternion.Inverse(projectionSurfaceRoot.rotation);
    }

    private void Update()
    {
      if (_grabbed)
      {
        return;
      }

      // Reset position relative to combined query term provider
      var t = transform;
      t.position = projectionSurfaceRoot.TransformPoint(_offset);
      t.rotation = projectionSurfaceRoot.rotation * _rotationOffset;
    }

    private void CreatePoseSkeleton()
    {
      var t = transform;
      var poseSkeleton = Instantiate(poseSkeletonPrefab, t.position, t.rotation);
      projectionSurface.AddPoseSkeleton(poseSkeleton);
    }

    public void OnGrab()
    {
      _grabbed = true;
    }

    public void OnDrop()
    {
      _grabbed = false;
      CreatePoseSkeleton();
    }
  }
}
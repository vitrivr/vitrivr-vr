using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseSkeletonController : MonoBehaviour
  {
    public KeyPointController[] KeyPoints { get; private set; }

    private void Awake()
    {
      KeyPoints = GetComponentsInChildren<KeyPointController>();
    }
  }
}
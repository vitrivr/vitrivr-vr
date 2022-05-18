using System;
using UnityEngine;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseSkeletonController : MonoBehaviour
  {
    public KeyPointController[] KeyPoints { get; private set; }

    public Action onClose = () => { };

    private void Awake()
    {
      KeyPoints = GetComponentsInChildren<KeyPointController>();
    }

    public void Close()
    {
      onClose();
      Destroy(gameObject);
    }
  }
}
using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class Grabable : EventInteractable
  {
    /// <summary>
    /// Allows setting the grab target to other transforms e.g. parent (defaults to this transform).
    /// </summary>
    public Transform grabTransform;

    private Vector3 _grabAnchor;
    private Quaternion _rotationAnchor;
    private Quaternion _inverseGrabberRotation;
    private Transform _grabber;

    protected void Awake()
    {
      if (grabTransform == null)
      {
        grabTransform = transform;
      }
    }

    protected void Update()
    {
      if (_grabber)
      {
        var rot = _grabber.rotation;
        grabTransform.position = _grabber.position + rot * _inverseGrabberRotation * _grabAnchor;
        grabTransform.rotation = rot * _rotationAnchor;
      }
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      base.OnGrab(interactor, start);
      _grabber = start ? interactor : null;
      if (start)
      {
        _grabAnchor = grabTransform.position - interactor.position;
        _inverseGrabberRotation = Quaternion.Inverse(interactor.rotation);
        _rotationAnchor = _inverseGrabberRotation * grabTransform.rotation;
      }
    }
  }
}
using UnityEngine;
using VitrivrVR.Interaction.System;

namespace VitrivrVR.Query.Term.Pose
{
  public class CanvasScaleHandle : EventInteractable
  {
    /// <summary>
    /// The scale target, in this case most likely the projection surface.
    /// </summary>
    public Transform target;

    private Transform _interactor;
    private bool _grabbed;
    private float _grabAnchorX, _grabAnchorY;
    private Vector3 _initialScale;

    private void Start()
    {
      _initialScale = target.localScale;
    }

    private void Update()
    {
      if (!_grabbed) return;

      var t = transform;
      // This only serves to preserve local space z value of the handle
      var localPosition = t.localPosition;
      // All calculations are performed in parent space as both the handle and projection surface are expected to share
      // the same parent
      var parentSpacePoint = t.parent.InverseTransformPoint(_interactor.position);
      // Ensure that the handle does not shrink the canvas beyond the minimum size
      localPosition.x = Mathf.Max(0, parentSpacePoint.x + _grabAnchorX);
      localPosition.y = Mathf.Min(0, parentSpacePoint.y + _grabAnchorY);
      t.localPosition = localPosition;

      // Perform actual canvas scaling
      target.localScale = _initialScale + new Vector3(localPosition.x, - localPosition.y, 0);
      target.localPosition = localPosition / 2;
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      base.OnGrab(interactor, start);
      if (start)
      {
        if (_grabbed) return;
        _interactor = interactor;
        var t = transform;
        var localPosition = t.localPosition;
        var parentSpacePoint = t.parent.InverseTransformPoint(_interactor.position);
        _grabAnchorX = localPosition.x - parentSpacePoint.x;
        _grabAnchorY = localPosition.y - parentSpacePoint.y;
        _grabbed = true;
      }
      else
      {
        if (interactor != _interactor) return;
        _interactor = null;
        _grabbed = false;
      }
    }
  }
}
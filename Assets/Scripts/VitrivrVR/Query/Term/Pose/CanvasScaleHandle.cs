using System;
using UnityEngine;
using VitrivrVR.Interaction.System;

namespace VitrivrVR.Query.Term.Pose
{
  public class CanvasScaleHandle : EventInteractable
  {
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
      var localPosition = t.localPosition;
      var parentSpacePoint = t.parent.InverseTransformPoint(_interactor.position);
      localPosition.x = Mathf.Max(0, parentSpacePoint.x + _grabAnchorX);
      localPosition.y = Mathf.Min(0, parentSpacePoint.y + _grabAnchorY);
      t.localPosition = localPosition;

      target.localScale = _initialScale + 2 * new Vector3(localPosition.x, - localPosition.y, 0);
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
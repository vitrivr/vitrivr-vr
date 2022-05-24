using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class GrabHandle : Interactable
  {
    private Transform _interactor;
    private bool _grabbed;
    private float _grabAnchor;

    private void Update()
    {
      if (!_grabbed) return;

      var t = transform;
      var localPosition = t.localPosition;
      localPosition.z = Mathf.Min(0, t.parent.InverseTransformPoint(_interactor.position).z + _grabAnchor);
      t.localPosition = localPosition;
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      if (start)
      {
        if (_grabbed) return;
        _interactor = interactor;
        var t = transform;
        _grabAnchor = t.localPosition.z - t.parent.InverseTransformPoint(_interactor.position).z;
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
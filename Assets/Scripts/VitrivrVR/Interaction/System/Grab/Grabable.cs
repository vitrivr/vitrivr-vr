using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VitrivrVR.Interaction.System.Grab
{
  public class Grabable : EventInteractable
  {
    [Serializable]
    public class HoverEvent : UnityEvent<bool>
    {
    }
    
    /// <summary>
    /// Allows setting the grab target to other transforms e.g. parent (defaults to this transform).
    /// </summary>
    public Transform grabTransform;

    /// <summary>
    /// Event that is triggered when the first interactor starts hovering over this interactable or when the last
    /// interactor stops hovering over this interactable.
    /// </summary>
    public HoverEvent onHoverChange;

    private Vector3 _grabAnchor;
    private Quaternion _rotationAnchor;
    private Quaternion _inverseGrabberRotation;
    private Transform _grabber;

    private readonly List<Transform> _hovering = new List<Transform>();

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
      _grabber = start ? interactor : null;
      if (start)
      {
        // grabAnchor = grabTransform.localPosition - grabTransform.InverseTransformPoint(interactor.position);
        _grabAnchor = grabTransform.position - interactor.position;
        _inverseGrabberRotation = Quaternion.Inverse(interactor.rotation);
        _rotationAnchor = _inverseGrabberRotation * grabTransform.rotation;
      }
    }

    public override void OnHoverEnter(Transform interactor)
    {
      _hovering.Add(interactor);
      if (_hovering.Count == 1)
      {
        onHoverChange.Invoke(true);
      }
    }
    
    public override void OnHoverExit(Transform interactor)
    {
      _hovering.Remove(interactor);
      if (_hovering.Count == 0)
      {
        onHoverChange.Invoke(false);
      }
    }
  }
}
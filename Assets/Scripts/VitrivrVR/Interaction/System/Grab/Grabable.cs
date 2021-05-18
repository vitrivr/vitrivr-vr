using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class Grabable : Interactable
  {
    /// <summary>
    /// Allows setting the grab target to other transforms e.g. parent (defaults to this transform).
    /// </summary>
    public Transform grabTransform;

    protected Vector3 grabAnchor;
    protected Transform grabber;

    private void Awake()
    {
      if (grabTransform == null)
      {
        grabTransform = transform;
      }
    }

    protected void Update()
    {
      if (grabber)
      {
        grabTransform.position = grabber.position + grabAnchor;
      }
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      grabber = start ? interactor : null;
      if (start)
      {
        // grabAnchor = grabTransform.localPosition - grabTransform.InverseTransformPoint(interactor.position);
        grabAnchor = grabTransform.position - interactor.position;
      }
    }
  }
}
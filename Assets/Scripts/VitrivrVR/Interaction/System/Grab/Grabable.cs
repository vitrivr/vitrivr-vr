using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class Grabable : Interactable
  {
    /// <summary>
    /// Allows setting the grab target to other transforms e.g. parent (defaults to this transform).
    /// </summary>
    public Transform grabTransform;

    protected Rigidbody rb;

    private void Awake()
    {
      if (grabTransform == null)
      {
        grabTransform = transform;
      }

      if (grabTransform.TryGetComponent<Rigidbody>(out var rb))
      {
        this.rb = rb;
      }
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      grabTransform.SetParent(start ? interactor : null);

      if (rb != null)
      {
        rb.isKinematic = start;
      }
    }
  }
}
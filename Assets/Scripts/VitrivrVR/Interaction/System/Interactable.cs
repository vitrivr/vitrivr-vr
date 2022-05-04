using UnityEngine;

namespace VitrivrVR.Interaction.System
{
  public abstract class Interactable : MonoBehaviour
  {
    [Tooltip("If enabled, this interactable will prevent an interactor hovering over it to act as a UI pointer.")]
    public bool disablesPointer = true;
    
    public virtual void OnInteraction(Transform interactor, bool start)
    {
    }

    public virtual void OnGrab(Transform interactor, bool start)
    {
    }

    public virtual void OnHoverEnter(Transform interactor)
    {
    }
    
    public virtual void OnHoverExit(Transform interactor)
    {
    }
  }
}
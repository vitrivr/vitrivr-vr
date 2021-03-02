using UnityEngine;

namespace VitrivrVR.Interaction.System
{
  public abstract class Interactable : MonoBehaviour
  {
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
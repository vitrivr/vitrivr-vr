using UnityEngine;

namespace VitrivrVR.Interaction
{
  public abstract class Interactable : MonoBehaviour
  {
    public virtual void OnInteraction(Transform interactor, bool start)
    {
    }
    
    public virtual void OnGrab(Transform interactor, bool start)
    {
    }
  }
}
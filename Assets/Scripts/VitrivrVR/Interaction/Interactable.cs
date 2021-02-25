using UnityEngine;

namespace VitrivrVR.Interaction
{
  public abstract class Interactable : MonoBehaviour
  {
    public virtual void OnInteraction(Transform interactor, bool start)
    {
    }
  }
}
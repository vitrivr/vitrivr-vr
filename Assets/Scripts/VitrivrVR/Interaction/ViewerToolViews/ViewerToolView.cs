using UnityEngine;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public abstract class ViewerToolView : MonoBehaviour
  {
    public virtual bool EnableRayInteractor { get; } = false;

    public virtual void OnTriggerButton(bool pressed)
    {
    }
  }
}
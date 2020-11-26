using UnityEngine;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public abstract class ViewerToolView : MonoBehaviour
  {
    public virtual bool EnableRayInteractor { get; } = false;
  }
}
using UnityEngine;

namespace VitrivrVR.Util
{
  /// <summary>
  /// Connects a canvas with the current main camera and then destroys itself.
  /// To be used in prefabs using a canvas to allow XR UI interaction.
  /// </summary>
  public class CanvasCameraHelper : MonoBehaviour
  {
    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      Destroy(this);
    }
  }
}
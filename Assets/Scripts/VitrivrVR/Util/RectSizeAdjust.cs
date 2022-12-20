using UnityEngine;

namespace VitrivrVR.Util
{
  /// <summary>
  /// Adjusts the size of the attached object to the size of the provided rect transform on start.
  /// </summary>
  public class RectSizeAdjust : MonoBehaviour
  {
    public RectTransform adjustmentSource;

    private void Start()
    {
      var t = transform;
      var scale = t.localScale;
      var sourceScale = adjustmentSource.rect;
      scale.x = sourceScale.width;
      scale.y = sourceScale.height;
      t.localScale = scale;
    }
  }
}
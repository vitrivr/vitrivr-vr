using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VitrivrVR.Media;

namespace VitrivrVR.Util
{
  /// <summary>
  /// Tiny class for the sole purpose of enabling click events on <see cref="CanvasMediaItemDisplay"/> instances.
  /// </summary>
  public class ClickHandler : MonoBehaviour, IPointerClickHandler
  {
    public Action<PointerEventData> onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
      onClick(eventData);
    }
  }
}
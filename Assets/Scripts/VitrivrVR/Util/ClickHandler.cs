using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VitrivrVR.Media;

namespace VitrivrVR.Util
{
  /// <summary>
  /// Tiny class for the sole purpose of enabling click events on <see cref="CanvasMediaItemDisplay"/> instances.
  /// </summary>
  public class ClickHandler : MonoBehaviour, IPointerClickHandler
  {
    [Serializable]
    public class ClickEvent : UnityEvent<PointerEventData>
    {
    }

    public ClickEvent onClickEvent;
    public Action<PointerEventData> onClick = data => { };

    public void OnPointerClick(PointerEventData eventData)
    {
      onClick(eventData);
      onClickEvent?.Invoke(eventData);
    }
  }
}
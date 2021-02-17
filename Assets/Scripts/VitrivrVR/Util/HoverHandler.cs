using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VitrivrVR.Util
{
  public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
  {
    public Action<PointerEventData> onEnter = data => { };
    public Action<PointerEventData> onExit = data => { };

    public void OnPointerEnter(PointerEventData eventData)
    {
      onEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      onExit(eventData);
    }
  }
}
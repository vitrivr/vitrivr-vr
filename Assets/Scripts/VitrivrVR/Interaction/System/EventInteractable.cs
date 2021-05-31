using System;
using UnityEngine;
using UnityEngine.Events;

namespace VitrivrVR.Interaction.System
{
  public class EventInteractable : Interactable
  {
    [Serializable]
    public class SimpleInteractionEvent : UnityEvent
    {
    }


    /// <summary>
    /// Event that is triggered on interaction.
    /// </summary>
    public SimpleInteractionEvent onInteractionEvent;

    public override void OnInteraction(Transform interactor, bool start)
    {
      if (start)
      {
        onInteractionEvent.Invoke();
      }
    }
  }
}
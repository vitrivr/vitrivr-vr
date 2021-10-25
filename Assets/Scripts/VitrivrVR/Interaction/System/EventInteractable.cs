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

    [Serializable]
    public class ComplexInteractionEvent : UnityEvent<Transform, bool>
    {
    }


    /// <summary>
    /// Event that is triggered on interaction.
    /// </summary>
    public ComplexInteractionEvent onInteraction;

    public SimpleInteractionEvent onGrab;
    public SimpleInteractionEvent onDrop;

    public override void OnInteraction(Transform interactor, bool start)
    {
      onInteraction.Invoke(interactor, start);
    }

    /// <summary>
    /// Event that is triggered on grab.
    /// </summary>
    /// <param name="interactor">Interactor in control of grab.</param>
    /// <param name="start">If interactable is being grabbed or let go.</param>
    public override void OnGrab(Transform interactor, bool start)
    {
      if (start)
      {
        onGrab.Invoke();
      }
      else
      {
        onDrop.Invoke();
      }
    }
  }
}
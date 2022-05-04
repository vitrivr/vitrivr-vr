using System;
using System.Collections.Generic;
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
    public class RegularInteractionEvent : UnityEvent<bool>
    {
    }

    [Serializable]
    public class ComplexInteractionEvent : UnityEvent<Transform, bool>
    {
    }

    [Tooltip("Event that is triggered on interactor interaction.")]
    public ComplexInteractionEvent onInteraction;

    [Tooltip("Event that is triggered on interactor grab.")]
    public SimpleInteractionEvent onGrab;

    [Tooltip("Event that is triggered on interactor drop (grab released).")]
    public SimpleInteractionEvent onDrop;

    [Tooltip("Event that is triggered when the first interactor starts hovering over this interactable or when the " +
             "last interactor stops hovering over this interactable.")]
    public RegularInteractionEvent onHoverChange;

    private readonly HashSet<Transform> _hovering = new();

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

    public override void OnHoverEnter(Transform interactor)
    {
      _hovering.Add(interactor);
      // Only trigger if this interactable isn't already being hovered.
      if (_hovering.Count == 1)
      {
        onHoverChange.Invoke(true);
      }
    }

    public override void OnHoverExit(Transform interactor)
    {
      _hovering.Remove(interactor);
      // Only trigger if this change results in this interactable no longer being hovered.
      if (_hovering.Count == 0)
      {
        onHoverChange.Invoke(false);
      }
    }

    private void OnDisable()
    {
      // Disabled interactables are not hoverable --> if currently hovered send hover change event and remove hovering
      if (_hovering.Count <= 0) return;
      onHoverChange.Invoke(false);
      _hovering.Clear();
    }
  }
}
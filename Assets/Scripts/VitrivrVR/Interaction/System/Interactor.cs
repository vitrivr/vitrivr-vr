using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Interaction.System
{
  public class Interactor : MonoBehaviour
  {
    private readonly List<Interactable> _interactables = new List<Interactable>();

    private List<Interactable> _grabbed;

    public void Interact(bool start)
    {
      foreach (var interactable in _interactables)
      {
        interactable.OnInteraction(transform, start);
      }
    }

    public void Grab(bool start)
    {
      if (start)
      {
        _grabbed = new List<Interactable>(_interactables);
        foreach (var interactable in _interactables)
        {
          interactable.OnGrab(transform, true);
        }
      }
      else
      {
        foreach (var interactable in _grabbed)
        {
          if (interactable)
          {
            interactable.OnGrab(transform, false);
          }
        }

        _grabbed = null;
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.TryGetComponent<Interactable>(out var interactable))
      {
        _interactables.Add(interactable);
        interactable.OnHoverEnter(transform);
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.TryGetComponent<Interactable>(out var interactable))
      {
        _interactables.Remove(interactable);
        interactable.OnHoverExit(transform);
      }
    }
  }
}
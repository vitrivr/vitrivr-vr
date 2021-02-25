using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Interaction
{
  public class Interactor : MonoBehaviour
  {
    private readonly List<Interactable> _interactables = new List<Interactable>();

    public void Interact(bool start)
    {
      foreach (var interactable in _interactables)
      {
        interactable.OnInteraction(transform, start);
      }
    }

    public void Grab(bool start)
    {
      foreach (var interactable in _interactables)
      {
        interactable.OnGrab(transform, start);
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.TryGetComponent<Interactable>(out var interactable))
      {
        _interactables.Add(interactable);
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.TryGetComponent<Interactable>(out var interactable))
      {
        _interactables.Remove(interactable);
      }
    }
  }
}
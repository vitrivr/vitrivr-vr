using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Interaction.System
{
  public class Interactor : MonoBehaviour
  {
    public InputAction interact;
    public InputAction grab;

    private readonly List<Interactable> _interactables = new List<Interactable>();

    private List<Interactable> _grabbed;

    private void Awake()
    {
      interact.started += context => Interact(context.ReadValueAsButton());
      interact.canceled += context => Interact(context.ReadValueAsButton());

      grab.started += context => Grab(context.ReadValueAsButton());
      grab.canceled += context => Grab(context.ReadValueAsButton());
    }

    private void OnEnable()
    {
      interact.Enable();
      grab.Enable();
    }

    private void OnDisable()
    {
      interact.Disable();
      grab.Disable();
    }

    public void Interact(bool start)
    {
      RemoveDestroyed();
      foreach (var interactable in _interactables)
      {
        interactable.OnInteraction(transform, start);
      }
    }

    public void Grab(bool start)
    {
      RemoveDestroyed();
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

    private void RemoveDestroyed()
    {
      _interactables.RemoveAll(interactable => interactable == null);
      _grabbed?.RemoveAll(interactable => interactable == null);
    }
  }
}
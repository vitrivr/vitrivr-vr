using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Interaction.System
{
  public class Interactor : MonoBehaviour
  {
    public InputAction interact;
    public InputAction grab;

    public InputActionReference uiClickAction;
    public int bindingIndex;
    public GameObject uiLine;

    private readonly HashSet<Interactable> _interactables = new();

    private HashSet<Interactable> _grabbed;
    private HashSet<Interactable> _interacting;

    private bool _uiPointerActive = true;

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
      RemoveDisabledOrDestroyed();
      if (start)
      {
        _interacting = new HashSet<Interactable>(_interactables);
        foreach (var interactable in _interactables)
        {
          interactable.OnInteraction(transform, true);
        }
      }
      else
      {
        foreach (var interactable in _interacting)
        {
          interactable.OnInteraction(transform, false);

          _interacting = null;
        }
      }
    }

    public void Grab(bool start)
    {
      RemoveDisabledOrDestroyed();
      if (start)
      {
        _grabbed = new HashSet<Interactable>(_interactables);
        foreach (var interactable in _interactables)
        {
          interactable.OnGrab(transform, true);
        }
      }
      else
      {
        foreach (var interactable in _grabbed.Where(interactable => interactable))
        {
          interactable.OnGrab(transform, false);
        }

        _grabbed = null;
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (!other.TryGetComponent<Interactable>(out var interactable)) return;

      _interactables.Add(interactable);
      interactable.OnHoverEnter(transform);

      UpdateUIPointer();
    }

    private void OnTriggerExit(Collider other)
    {
      if (!other.TryGetComponent<Interactable>(out var interactable)) return;

      _interactables.Remove(interactable);
      interactable.OnHoverExit(transform);

      UpdateUIPointer();
    }

    private void FixedUpdate()
    {
      // Reduce chance of interactables containing an interactable this interactor is no longer over
      RemoveDisabledOrDestroyed();
      UpdateUIPointer();
    }

    private void RemoveDisabledOrDestroyed()
    {
      _interactables.Remove(null);
      _interactables.RemoveWhere(interactable => !interactable.gameObject.activeInHierarchy);
      _grabbed?.Remove(null);
      _interacting?.Remove(null);
    }

    private void UpdateUIPointer()
    {
      var shouldBeDisabled = _interactables.Any(interactable => interactable.disablesPointer);

      switch (shouldBeDisabled)
      {
        case true when _uiPointerActive:
          uiClickAction.action.ApplyBindingOverride(bindingIndex, string.Empty);
          _uiPointerActive = false;
          uiLine.SetActive(false);
          break;
        case false when !_uiPointerActive:
          uiClickAction.action.RemoveBindingOverride(bindingIndex);
          _uiPointerActive = true;
          uiLine.SetActive(true);
          break;
      }
    }
  }
}
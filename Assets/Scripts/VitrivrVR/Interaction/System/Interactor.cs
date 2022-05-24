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
    public int clickBindingIndex;
    public InputActionReference uiPointAction;
    public int pointBindingIndex;
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

    private static bool GetInteractable(Collider collider, out Interactable interactable)
    {
      // Check collider for interactable, then check attached rigidbody
      return collider.TryGetComponent(out interactable) || collider.attachedRigidbody.TryGetComponent(out interactable);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (!GetInteractable(other, out var interactable)) return;

      if (!_interactables.Add(interactable)) return;

      interactable.OnHoverEnter(transform);
      UpdateUIPointer();
    }

    private void OnTriggerExit(Collider other)
    {
      if (!GetInteractable(other, out var interactable)) return;

      if (!_interactables.Remove(interactable)) return;

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
      _interactables.RemoveWhere(interactable => interactable == null);
      _interactables.RemoveWhere(interactable => !interactable.gameObject.activeInHierarchy);
      _grabbed?.RemoveWhere(interactable => interactable == null);
      _interacting?.RemoveWhere(interactable => interactable == null);
    }

    private void UpdateUIPointer()
    {
      var shouldBeDisabled = _interactables.Any(interactable => interactable.disablesPointer);

      switch (shouldBeDisabled)
      {
        case true when _uiPointerActive:
          uiClickAction.action.ApplyBindingOverride(clickBindingIndex, string.Empty);
          uiPointAction.action.ApplyBindingOverride(pointBindingIndex, string.Empty);
          _uiPointerActive = false;
          uiLine.SetActive(false);
          break;
        case false when !_uiPointerActive:
          uiClickAction.action.RemoveBindingOverride(clickBindingIndex);
          uiPointAction.action.RemoveBindingOverride(pointBindingIndex);
          _uiPointerActive = true;
          uiLine.SetActive(true);
          break;
      }
    }
  }
}
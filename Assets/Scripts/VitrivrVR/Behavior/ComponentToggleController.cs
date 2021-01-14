using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Behavior
{
  /// <summary>
  /// Class allowing the easy toggling of two sets of components.
  /// </summary>
  public class ComponentToggleController : MonoBehaviour
  {
    /// <summary>
    /// List of components that are initially enabled and are disabled when toggled.
    /// </summary>
    public List<Behaviour> positiveComponents;

    /// <summary>
    /// List of components that are initially disabled and are enabled when toggled.
    /// </summary>
    public List<Behaviour> negativeComponents;

    private bool _toggleState = true;

    private void Start()
    {
      Toggle(_toggleState);
    }

    public void Toggle()
    {
      _toggleState = !_toggleState;
      Toggle(_toggleState);
    }

    public void Toggle(bool toggleState)
    {
      foreach (var component in positiveComponents)
      {
        component.enabled = toggleState;
      }

      foreach (var component in negativeComponents)
      {
        component.enabled = !toggleState;
      }
    }
  }
}
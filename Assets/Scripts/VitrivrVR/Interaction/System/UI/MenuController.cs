using System.Collections.Generic;
using UnityEngine;

namespace VitrivrVR.Interaction.System.UI
{
  /// <summary>
  /// Basic interactable menu controller to allow toggling of menu items.
  /// </summary>
  public class MenuController : Interactable
  {
    public List<Transform> menuItems;

    private bool _showing;

    public override void OnInteraction(Transform _, bool start)
    {
      if (!start) return;
      _showing = !_showing;
      foreach (var menuItem in menuItems)
      {
        menuItem.gameObject.SetActive(_showing);
      }
    }
  }
}
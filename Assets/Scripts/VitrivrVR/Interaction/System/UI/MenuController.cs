using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Interaction.System.UI
{
  /// <summary>
  /// Basic interactable menu controller to allow toggling of menu items.
  /// </summary>
  public class MenuController : MonoBehaviour
  {
    public InputAction toggleAction;

    public List<Transform> menuItems;

    private bool _showing;

    private void Awake()
    {
      toggleAction.performed += ToggleMenu;
    }

    private void OnEnable()
    {
      toggleAction.Enable();
    }

    private void OnDisable()
    {
      toggleAction.Disable();
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
      _showing = !_showing;
      if (_showing)
      {
        var t = transform;
        var ct = Camera.main!.transform;
        
        t.rotation = Quaternion.Euler(0, ct.rotation.eulerAngles.y, 0);
        t.position = ct.position;
      }

      foreach (var menuItem in menuItems)
      {
        menuItem.gameObject.SetActive(_showing);
      }
    }
  }
}
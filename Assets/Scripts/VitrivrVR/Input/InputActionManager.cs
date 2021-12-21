using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Input
{
  public class InputActionManager : MonoBehaviour
  {
    public List<InputActionAsset> inputActionAssets;

    private void OnEnable()
    {
      foreach (var inputActionAsset in inputActionAssets)
      {
        inputActionAsset.Enable();
      }
    }

    private void OnDisable()
    {
      foreach (var inputActionAsset in inputActionAssets)
      {
        inputActionAsset.Disable();
      }
    }
  }
}
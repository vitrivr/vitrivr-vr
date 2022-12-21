using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Behavior
{
  public class HandBehavior : MonoBehaviour
  {
    public InputAction grip;

    [Tooltip("The model to scale by the grip action.")]
    public Transform model;

    private const float ReducedScale = 0.5f;
    private Vector3 _initialScale;

    private void Start()
    {
      _initialScale = model.localScale;
      grip.performed += ScaleModel;
    }

    private void OnEnable()
    {
      grip.Enable();
    }

    private void OnDisable()
    {
      grip.Disable();
    }

    private void ScaleModel(InputAction.CallbackContext context)
    {
      model.localScale = _initialScale * (1 - ReducedScale * context.ReadValue<float>());
    }
  }
}
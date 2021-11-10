using System;
using DeepSpeech;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VitrivrVR.Input.Text
{
  public class DeepSpeechDictationController : DeepSpeechStreamController
  {
    [Serializable]
    public class DictationStateEvent : UnityEvent<bool>
    {
    }

    public DictationStateEvent onDictationStateChange;
    public InputAction dictateAction;

    private new void Start()
    {
      base.Start();

      dictateAction.performed += SetDictation;
      dictateAction.canceled += SetDictation;
    }

    private void OnEnable()
    {
      dictateAction.Enable();
    }

    private void OnDisable()
    {
      dictateAction.Disable();
    }

    public void SetDictation(InputAction.CallbackContext context)
    {
      var startDictation = context.performed;

      onDictationStateChange.Invoke(startDictation);
      if (startDictation)
      {
        StartDictation();
      }
      else
      {
        StopDictation();
      }
    }
  }
}
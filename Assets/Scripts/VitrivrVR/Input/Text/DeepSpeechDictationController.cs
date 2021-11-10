using System;
using DeepSpeech;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VitrivrVR.Input.Text
{
  public class DeepSpeechDictationController : MonoBehaviour
  {
    [Serializable]
    public class DictationStateEvent : UnityEvent<bool>
    {
    }

    public DictationStateEvent onDictationStateChange;
    public InputAction dictateAction;

    private DeepSpeechStreamController _deepSpeech;

    private void Start()
    {
      _deepSpeech = GetComponent<DeepSpeechStreamController>();

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
        _deepSpeech.StartDictation();
      }
      else
      {
        _deepSpeech.StopDictation();
      }
    }
  }
}
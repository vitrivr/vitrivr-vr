using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using VitrivrVR.Config;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Object for text input through dictation.
  /// <para>Place in a scene, connect actions to the respective dictation events and use public methods to start or stop
  /// dictation.</para>
  /// </summary>
  public class DictationController : MonoBehaviour
  {
    [Serializable]
    public class DictationResultEvent : UnityEvent<string, ConfidenceLevel>
    {
    }

    [Serializable]
    public class DictationHypothesisEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class DictationCompleteEvent : UnityEvent<DictationCompletionCause>
    {
    }

    [Serializable]
    public class DictationErrorEvent : UnityEvent<string, int>
    {
    }

    [Serializable]
    public class DictationStateEvent : UnityEvent<bool>
    {
    }

    /// <summary>
    /// Event triggered when a dictation result is available. Provides the dictation result and the recognition
    /// confidence.
    /// </summary>
    public DictationResultEvent onDictationResult;

    /// <summary>
    /// Event triggered when a dictation hypothesis is available. Dictation hypotheses are usually only partial and
    /// should in most cases only be used for display purposes. Provides dictation hypothesis.
    /// </summary>
    public DictationHypothesisEvent onDictationHypothesis;

    /// <summary>
    /// Event triggered when the dictation session is completed. Provides completion cause.
    /// </summary>
    public DictationCompleteEvent onDictationComplete;

    /// <summary>
    /// Event triggered when an error is encountered during dictation. Provides the error as string and the
    /// corresponding error code.
    /// </summary>
    public DictationErrorEvent onDictationError;

    /// <summary>
    /// Event triggered when the dictation state changes.
    /// </summary>
    public DictationStateEvent onDictationStateChange;

    private DictationRecognizer _dictationRecognizer;

    private void Awake()
    {
      // Set up dictation
      _dictationRecognizer = new DictationRecognizer();

      // Register dictation events
      _dictationRecognizer.DictationResult += (text, confidence) =>
      {
        if (ConfigManager.Config.dictationDebugOutput)
        {
          Debug.Log($"{text}: {confidence}");
        }

        onDictationResult.Invoke(text, confidence);
      };

      _dictationRecognizer.DictationHypothesis += text =>
      {
        if (ConfigManager.Config.dictationDebugOutput)
        {
          Debug.Log(text);
        }

        onDictationHypothesis.Invoke(text);
      };

      _dictationRecognizer.DictationComplete += completionCause =>
      {
        if (ConfigManager.Config.dictationDebugOutput)
        {
          Debug.Log(completionCause);
        }

        onDictationComplete.Invoke(completionCause);
      };

      _dictationRecognizer.DictationError += (error, hresult) =>
      {
        Debug.LogError($"{error}: {hresult}");
        onDictationError.Invoke(error, hresult);
      };
    }

    public void SetDictation(InputAction.CallbackContext context)
    {
      Debug.Log("Test!");
      SetDictation(context.performed);
    }

    public void SetDictation(bool dictation)
    {
      if (dictation)
      {
        StartDictation();
      }
      else
      {
        StopDictation();
      }
    }

    public bool IsListening()
    {
      return _dictationRecognizer.Status == SpeechSystemStatus.Running;
    }

    private void StartDictation()
    {
      if (_dictationRecognizer.Status == SpeechSystemStatus.Running)
      {
        Debug.LogError("Dictation already running!");
        return;
      }

      _dictationRecognizer.Start();

      onDictationStateChange.Invoke(true);
    }

    private void StopDictation()
    {
      if (_dictationRecognizer.Status != SpeechSystemStatus.Running)
      {
        Debug.LogError("Dictation not running, cannot stop!");
        return;
      }

      _dictationRecognizer.Stop();

      onDictationStateChange.Invoke(false);
    }
  }
}
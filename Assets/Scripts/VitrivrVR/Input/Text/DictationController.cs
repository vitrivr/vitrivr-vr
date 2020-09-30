using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using VitrivrVR.Input.Controller;

namespace VitrivrVR.Input.Text
{
  public class DictationController : MonoBehaviour
  {
    [Serializable]
    public class DictationResultEvent : UnityEvent<string, ConfidenceLevel> { }
    [Serializable]
    public class DictationHypothesisEvent : UnityEvent<string> { }
    [Serializable]
    public class DictationCompleteEvent : UnityEvent<DictationCompletionCause> { }
    [Serializable]
    public class DictationErrorEvent : UnityEvent<string, int> { }
    
    public DictationResultEvent onDictationResult;
    public DictationHypothesisEvent onDictationHypothesis;
    public DictationCompleteEvent onDictationComplete;
    public DictationErrorEvent onDictationError;

    private DictationRecognizer _dictationRecognizer;

    private void Awake()
    {
      // Set up dictation
      _dictationRecognizer = new DictationRecognizer();

      _dictationRecognizer.DictationResult += (text, confidence) =>
      {
        Debug.Log($"{text}: {confidence}");
        // if (confidence == ConfidenceLevel.High || confidence == ConfidenceLevel.Medium)
        // {
        //   if (textField.text.Length > 0)
        //   {
        //     textField.text += " ";
        //   }
        //
        //   textField.text += text;
        // }
        
        onDictationResult.Invoke(text, confidence);
      };

      _dictationRecognizer.DictationHypothesis += text =>
      {
        Debug.Log(text);
        // previewText.text = text;
        onDictationHypothesis.Invoke(text);
      };

      _dictationRecognizer.DictationComplete += completionCause =>
      {
        Debug.Log(completionCause);
        // previewText.text = "";
        onDictationComplete.Invoke(completionCause);
      };

      _dictationRecognizer.DictationError += (error, hresult) =>
      {
        Debug.LogError($"{error}: {hresult}");
        onDictationError.Invoke(error, hresult);
      };
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

    public void StartDictation()
    {
      if (_dictationRecognizer.Status == SpeechSystemStatus.Running)
      {
        Debug.Log("Dictation already running!");
        return;
      }

      _dictationRecognizer.Start();
    }

    public void StopDictation()
    {
      if (_dictationRecognizer.Status != SpeechSystemStatus.Running)
      {
        Debug.Log("Dictation not running, cannot stop!");
        return;
      }

      _dictationRecognizer.Stop();
    }
  }
}
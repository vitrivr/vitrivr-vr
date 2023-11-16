using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Whisper;
using Whisper.Utils;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Stream transcription from microphone input.
  /// Adapted from Whisper for Unity UPM package.
  /// </summary>
  public class WhisperDictationController : MonoBehaviour
  {
    [Serializable]
    public class DictationStateEvent : UnityEvent<bool>
    {
    }

    [Serializable]
    public class WhisperResultEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class WhisperPredictionEvent : UnityEvent<string>
    {
    }

    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;

    [Header("Input")] public InputAction dictateAction;

    [Header("Output")] public DictationStateEvent onDictationStateChange;

    [Tooltip("Called when the prediction of the current segment is updated.")]
    public WhisperPredictionEvent onSegmentPrediction;

    [Tooltip("Called when the prediction of the final result is updated.")]
    public WhisperPredictionEvent onResultPrediction;

    [Tooltip("Called when the final result of the current segment is determined.")]
    public WhisperResultEvent onSegmentResult;

    [Tooltip("Called when the final result of the stream is determined.")]
    public WhisperResultEvent onResult;


    private WhisperStream _stream;

    private async void Start()
    {
      _stream = await whisper.CreateStream(microphoneRecord);
      _stream.OnResultUpdated += OnResultUpdated;
      _stream.OnSegmentUpdated += OnSegmentUpdated;
      _stream.OnSegmentFinished += OnSegmentFinished;
      _stream.OnStreamFinished += OnStreamFinished;

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

    private void SetDictation(InputAction.CallbackContext context)
    {
      var startDictation = context.performed;

      onDictationStateChange.Invoke(startDictation);

      if (startDictation)
      {
        _stream.StartStream();
        microphoneRecord.StartRecord();
      }
      else
      {
        microphoneRecord.StopRecord();
      }
    }

    private void OnResultUpdated(string result)
    {
      onResultPrediction.Invoke(result);
    }

    private void OnSegmentUpdated(WhisperResult segment)
    {
      onSegmentPrediction.Invoke(segment.Result);
    }

    private void OnSegmentFinished(WhisperResult segment)
    {
      onSegmentResult.Invoke(segment.Result);
    }

    private void OnStreamFinished(string finalResult)
    {
      onResult.Invoke(finalResult);
    }
  }
}
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
    
        [Header("Input")]
        public InputAction dictateAction;
        
        [Header("Output")]
        public DictationStateEvent onDictationStateChange;
        public WhisperResultEvent onResult;
        public WhisperPredictionEvent onPrediction;
        
        private WhisperStream _stream;

        private async void Start()
        {
            _stream = await whisper.CreateStream(microphoneRecord);
            _stream.OnResultUpdated += OnResult;
            _stream.OnSegmentUpdated += OnSegmentUpdated;
            _stream.OnSegmentFinished += OnSegmentFinished;
            _stream.OnStreamFinished += OnFinished;

            dictateAction.performed += OnButtonPressed;
        }
        
        private void OnEnable()
        {
            dictateAction.Enable();
        }

        private void OnDisable()
        {
            dictateAction.Disable();
        }

        private void OnButtonPressed(InputAction.CallbackContext callbackContext)
        {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
            }
            else
                microphoneRecord.StopRecord();
        
            onDictationStateChange.Invoke(microphoneRecord.IsRecording);
        }
    
    
        private void OnResult(string result)
        {
            print($"Result: {result}");
            onResult.Invoke(result);
        }
        
        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
            onPrediction.Invoke(segment.Result);
        }
        
        private void OnSegmentFinished(WhisperResult segment)
        {
            print($"Segment finished: {segment.Result}");
            onPrediction.Invoke(segment.Result);
        }
        
        private void OnFinished(string finalResult)
        {
            print($"Stream finished! {finalResult}");
        }
    }
}

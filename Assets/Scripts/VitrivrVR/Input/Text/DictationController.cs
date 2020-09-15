using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;
using VitrivrVR.Input.Controller;

namespace VitrivrVR.Input.Text
{
    public class DictationController : MonoBehaviour
    {
        public TextMeshPro previewText;
        public TMP_InputField textField;
        public XRButtonObserver buttonObserver;
    
        private DictationRecognizer _dictationRecognizer;

        private void Awake()
        {
            // Set up dictation
            _dictationRecognizer = new DictationRecognizer();
        
            _dictationRecognizer.DictationResult += (text, confidence) =>
            {
                Debug.Log($"{text}: {confidence}");
                if (confidence == ConfidenceLevel.High || confidence == ConfidenceLevel.Medium)
                {
                    if (textField.text.Length > 0)
                    {
                        textField.text += " ";
                    }
                    textField.text += text;
                }
            };

            _dictationRecognizer.DictationHypothesis += text =>
            {
                Debug.Log(text);
                previewText.text = text;
            };

            _dictationRecognizer.DictationComplete += completionCause =>
            {
                Debug.Log(completionCause);
                previewText.text = "";
            };

            _dictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogError($"{error}: {hresult}");
            };
        
            // Register with button observer
            buttonObserver.primaryButtonEvent.AddListener(SetDictation);
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

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

namespace VitrivrVR.Input.Text
{
    public class TextInputController : MonoBehaviour
    {
        public TMP_InputField textField;
        public Button keyboardButtonPrefab;
        public Canvas keyboardCanvasPrefab;
        public string[] keys;

        private float _buttonSize;
        private Canvas _keyboard;

        private DictationRecognizer _dictationRecognizer;

        void Awake()
        {
            var buttonRect = keyboardButtonPrefab.GetComponent<RectTransform>();
            _buttonSize = buttonRect.rect.height;
        
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
            };

            _dictationRecognizer.DictationComplete += completionCause =>
            {
                Debug.Log(completionCause);
            };

            _dictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogError($"{error}: {hresult}");
            };
        }

        public void ShowKeyboard()
        {
            if (_keyboard != null)
            {
                return;
            }

            _keyboard = Instantiate(keyboardCanvasPrefab, new Vector3(0, 1,-0.1f ), Quaternion.identity);
        
            _keyboard.worldCamera = Camera.main;

            var j = 0;
            foreach (var row in keys)
            {
                var i = 0;
                foreach (var key in row)
                {
                    var button = Instantiate(keyboardButtonPrefab, _keyboard.transform);
                    if (key == '<')
                    {
                        button.onClick.AddListener(() =>
                        {
                            if (textField.text.Length > 0)
                            {
                                var caretPosition = textField.caretPosition;
                                if (caretPosition == 0)
                                {
                                    caretPosition = textField.text.Length;
                                }
                                textField.text = textField.text.Remove(caretPosition - 1, 1);
                                textField.caretPosition = Mathf.Max(caretPosition - 1, 0);
                            }
                        });
                    }
                    else
                    {
                        button.onClick.AddListener(() => textField.text += key);
                    }
                    var buttonText = button.GetComponentInChildren<TMP_Text>();
                    buttonText.text = key.ToString().ToUpper();
                    var buttonRect = button.GetComponent<RectTransform>();
                    buttonRect.anchoredPosition = new Vector2(i * _buttonSize, -j * _buttonSize);
                    i++;
                }
                j++;
            }
        
            CreateKey("_", 4, 0, j, () => textField.text += ' ');
            CreateKey("Done", 2, 4, j, HideKeyboard);
            CreateKey("Rec", 2, 6, j, ToggleDictation);
            CreateKey("Clear", 2, 8, j, () => textField.text = "");
        }

        public void HideKeyboard()
        {
            Destroy(_keyboard.gameObject);
            _keyboard = null;
        }

        public void ToggleDictation()
        {
            if (_dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                StopDictation();
            }
            else
            {
                StartDictation();
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

        private void CreateKey(string text, int width, int column, int row, UnityAction onClick)
        {
            var button = Instantiate(keyboardButtonPrefab, _keyboard.transform);
            button.onClick.AddListener(onClick);
            var buttonText = button.GetComponentInChildren<TMP_Text>();
            buttonText.text = text;
            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(column * _buttonSize, -row * _buttonSize);
            buttonRect.sizeDelta = new Vector2(width * _buttonSize, _buttonSize);
        }
    }
}

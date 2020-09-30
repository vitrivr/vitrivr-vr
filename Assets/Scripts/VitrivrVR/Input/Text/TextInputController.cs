using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

namespace VitrivrVR.Input.Text
{
  public class TextInputController : MonoBehaviour
  {
    public TMP_InputField tagTextField;
    public TMP_InputField textTextField;
    public Button keyboardButtonPrefab;
    public Canvas keyboardCanvasPrefab;
    public string[] keys;

    private float _buttonSize;
    private Canvas _keyboard;

    void Awake()
    {
      var buttonRect = keyboardButtonPrefab.GetComponent<RectTransform>();
      _buttonSize = buttonRect.rect.height;
    }

    public void ReceiveDictationResult(string text, ConfidenceLevel confidence)
    {
      if (confidence != ConfidenceLevel.High && confidence != ConfidenceLevel.Medium) return;
      
      var textField = GetSelectedTextField();

      if (textField == null)
      {
        return;
      }
        
      if (textField.text.Length > 0)
      {
        textField.text += " ";
      }
      
      textField.text += text;
    }

    private TMP_InputField GetSelectedTextField()
    {
      return tagTextField.isFocused ? tagTextField : textTextField.isFocused ? textTextField : null;
    }

    public void ShowKeyboard()
    {
      if (_keyboard != null)
      {
        return;
      }
      // TODO: Find good way to determine which text ¯box the text should be added to
      var textField = GetSelectedTextField();

      _keyboard = Instantiate(keyboardCanvasPrefab, new Vector3(0, 1, -0.1f), Quaternion.identity);

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
      CreateKey("Clear", 2, 6, j, () => textField.text = "");
    }

    public void HideKeyboard()
    {
      Destroy(_keyboard.gameObject);
      _keyboard = null;
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
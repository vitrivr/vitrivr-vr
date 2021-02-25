using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Canvas based text input controller that can also receive text input from a <see cref="DictationController"/>.
  /// </summary>
  public class CanvasTextInputController : MonoBehaviour
  {
    public TMP_InputField tagTextField;
    public TMP_InputField textTextField;
    public Button keyboardButtonPrefab;
    public Canvas keyboardCanvasPrefab;
    public string[] keys;

    private float _buttonSize;
    private Canvas _keyboard;

    private void Awake()
    {
      var buttonRect = keyboardButtonPrefab.GetComponent<RectTransform>();
      _buttonSize = buttonRect.rect.height;
    }

    /// <summary>
    /// Adds the dictation result to the currently selected text field. Will only process dictation results with High or
    /// Medium <see cref="ConfidenceLevel"/>.
    /// </summary>
    public void ReceiveDictationResult(string text, ConfidenceLevel confidence)
    {
      if (confidence != ConfidenceLevel.High && confidence != ConfidenceLevel.Medium) return;

      var textField = GetSelectedTextField();

      if (textField == null)
      {
        return; // No text field selected, nothing to do
      }

      if (textField.text.Length > 0)
      {
        textField.text += " "; // If there is preexisting text, append space
      }

      textField.text += text;
    }

    public void ReceiveTextInput(string text)
    {
      var textField = GetSelectedTextField();

      if (textField == null)
      {
        return; // No text field selected, nothing to do
      }

      textField.ProcessEvent(Event.KeyboardEvent(text));
      textField.ForceLabelUpdate();
    }

    public void ReceiveBackspace()
    {
      var textField = GetSelectedTextField();

      if (textField == null)
      {
        return; // No text field selected, nothing to do
      }

      textField.ProcessEvent(Event.KeyboardEvent("backspace"));
      textField.ForceLabelUpdate();
    }

    private TMP_InputField GetSelectedTextField()
    {
      return tagTextField.isFocused ? tagTextField : textTextField.isFocused ? textTextField : null;
    }

    /// <summary>
    /// Shows a <see cref="Canvas"/> based keyboard for very simple XR based text input with configurable keys.
    /// </summary>
    public void ShowKeyboard()
    {
      if (_keyboard != null)
      {
        return;
      }

      // TODO: Find good way to determine which text box the text should be added to
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
              if (textField.text.Length <= 0) return;
              var caretPosition = textField.caretPosition;
              if (caretPosition == 0)
              {
                caretPosition = textField.text.Length;
              }

              textField.text = textField.text.Remove(caretPosition - 1, 1);
              textField.caretPosition = Mathf.Max(caretPosition - 1, 0);
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
      if (_keyboard == null) return;
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
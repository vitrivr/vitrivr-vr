using Org.Vitrivr.DresApi.Model;
using UnityEngine;
using UnityEngine.Windows.Speech;
using VitrivrVR.Config;
using VitrivrVR.Submission;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// MonoBehaviour helper to input text into <see cref="TextInputManager"/>.
  /// </summary>
  public class SceneTextInputController : MonoBehaviour
  {
    public void InputText(string text)
    {
      TextInputManager.InputText(text);
      DresClientManager.LogInteraction("keyboard", $"input {text}", QueryEvent.CategoryEnum.TEXT);
    }

    public void InputBackspace()
    {
      TextInputManager.InputBackspace();
      DresClientManager.LogInteraction("keyboard", "backspace", QueryEvent.CategoryEnum.TEXT);
    }

    /// <summary>
    /// Adds the dictation result to the currently selected text field. Will only process dictation results with High or
    /// Medium <see cref="ConfidenceLevel"/>.
    /// </summary>
    public void ReceiveDictationResult(string text, ConfidenceLevel confidence)
    {
      if (confidence != ConfidenceLevel.High && confidence != ConfidenceLevel.Medium) return;

      InputText(text);
      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogInteraction("speechToText", $"input {text} {confidence.ToString()}",
          QueryEvent.CategoryEnum.TEXT);
      }
    }
  }
}
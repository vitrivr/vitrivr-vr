using UnityEngine;
using UnityEngine.Windows.Speech;

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
    }

    public void InputBackspace()
    {
      TextInputManager.InputBackspace();
    }

    /// <summary>
    /// Adds the dictation result to the currently selected text field. Will only process dictation results with High or
    /// Medium <see cref="ConfidenceLevel"/>.
    /// </summary>
    public void ReceiveDictationResult(string text, ConfidenceLevel confidence)
    {
      if (confidence != ConfidenceLevel.High && confidence != ConfidenceLevel.Medium) return;

      InputText(text);
    }
  }
}
using UnityEngine;
using VitrivrVR.Logging;

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
      LoggingController.LogInteraction("keyboard", $"input {text}", Logging.Interaction.TextInput);
    }

    public void InputBackspace()
    {
      TextInputManager.InputBackspace();
      LoggingController.LogInteraction("keyboard", "backspace", Logging.Interaction.TextInput);
    }

    public void InputReturn()
    {
      TextInputManager.InputReturn();
      LoggingController.LogInteraction("keyboard", "return", Logging.Interaction.TextInput);
    }

    public void InputLeftArrow()
    {
      TextInputManager.InputLeftArrow();
      LoggingController.LogInteraction("keyboard", "ArrowLeft", Logging.Interaction.TextInput);
    }

    public void InputRightArrow()
    {
      TextInputManager.InputRightArrow();
      LoggingController.LogInteraction("keyboard", "ArrowRight", Logging.Interaction.TextInput);
    }

    public void InputTabulator()
    {
      TextInputManager.InputTabulator();
      LoggingController.LogInteraction("keyboard", "Tabulator", Logging.Interaction.TextInput);
    }

    public void ReceiveDictationResult(string text)
    {
      InputText(text);
      LoggingController.LogInteraction("speechToText", $"input {text} DeepSpeech", Logging.Interaction.TextInput);
    }
  }
}
using UnityEngine;
using VitrivrVR.Config;
using Dev.Dres.ClientApi.Model;
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
      LoggingController.LogInteraction("keyboard", $"input {text}", QueryEvent.CategoryEnum.TEXT);
    }

    public void InputBackspace()
    {
      TextInputManager.InputBackspace();
      LoggingController.LogInteraction("keyboard", "backspace", QueryEvent.CategoryEnum.TEXT);
    }
    
    public void InputReturn()
    {
      TextInputManager.InputReturn();
      LoggingController.LogInteraction("keyboard", "return", QueryEvent.CategoryEnum.TEXT);
    }
    
    public void InputLeftArrow()
    {
      TextInputManager.InputLeftArrow();
      LoggingController.LogInteraction("keyboard", "ArrowLeft", QueryEvent.CategoryEnum.TEXT);
    }
    
    public void InputRightArrow()
    {
      TextInputManager.InputRightArrow();
      LoggingController.LogInteraction("keyboard", "ArrowRight", QueryEvent.CategoryEnum.TEXT);
    }
    public void InputTabulator()
    {
      TextInputManager.InputTabulator();
      LoggingController.LogInteraction("keyboard", "Tabulator", QueryEvent.CategoryEnum.TEXT);
    }

    public void ReceiveDictationResult(string text)
    {
      InputText(text);
      if (ConfigManager.Config.dresEnabled)
      {
        LoggingController.LogInteraction("speechToText", $"input {text} DeepSpeech", QueryEvent.CategoryEnum.TEXT);
      }
    }
  }
}
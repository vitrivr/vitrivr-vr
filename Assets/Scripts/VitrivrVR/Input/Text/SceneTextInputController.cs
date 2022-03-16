using Org.Vitrivr.DresApi.Model;
using UnityEngine;
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
    
    public void InputReturn()
    {
      TextInputManager.InputReturn();
      DresClientManager.LogInteraction("keyboard", "return", QueryEvent.CategoryEnum.TEXT);
    }
    
    public void InputLeftArrow()
    {
      TextInputManager.InputLeftArrow();
      DresClientManager.LogInteraction("keyboard", "ArrowLeft", QueryEvent.CategoryEnum.TEXT);
    }
    
    public void InputRightArrow()
    {
      TextInputManager.InputRightArrow();
      DresClientManager.LogInteraction("keyboard", "ArrowRight", QueryEvent.CategoryEnum.TEXT);
    }
    public void InputTabulator()
    {
      TextInputManager.InputTabulator();
      DresClientManager.LogInteraction("keyboard", "Tabulator", QueryEvent.CategoryEnum.TEXT);
    }

    public void ReceiveDictationResult(string text)
    {
      InputText(text);
      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogInteraction("speechToText", $"input {text} DeepSpeech", QueryEvent.CategoryEnum.TEXT);
      }
    }
  }
}
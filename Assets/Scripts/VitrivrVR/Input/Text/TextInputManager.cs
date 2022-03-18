using System.Collections.Generic;
using System.Linq;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Static class to allow routing of non-system text input to the currently selected registered text input field.
  /// This applies mostly to custom implemented XR based text input.
  /// </summary>
  public static class TextInputManager
  {
    private static List<InputFieldController> _inputFields = new List<InputFieldController>();
    
    /// <summary>
    /// Registers the given input field to receive text input from non-system sources.
    /// </summary>
    public static void Register(InputFieldController inputField)
    {
      _inputFields.Add(inputField);
    }

    /// <summary>
    /// Unregisters the given input field from receiving text input from non-system sources.
    /// </summary>
    public static void Unregister(InputFieldController inputField)
    {
      _inputFields.Remove(inputField);
    }

    public static void InputText(string text)
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputText(text);
    }
    
    public static void InputBackspace()
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputBackspace();
    }
    
    public static void InputReturn()
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputReturn();
    }
    
    public static void InputLeftArrow()
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputLeftArrow();
    }
    
    public static void InputRightArrow()
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputRightArrow();
    }
    
    public static void InputTabulator()
    {
      var inputField = GETSelectedInputField();
      
      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.InputTabulator();
    }

    private static InputFieldController GETSelectedInputField()
    {
      return _inputFields.FirstOrDefault(inputField => inputField.IsFocused);
    }
  }
}

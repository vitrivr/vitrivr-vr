using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Static class to allow routing of non-system text input to the currently selected registered text input field.
  /// This applies mostly to custom implemented XR based text input.
  /// </summary>
  public static class TextInputManager
  {
    /// <summary>
    /// Inputs the given text into the currently selected text input field.
    /// </summary>
    /// <param name="text"></param>
    public static void InputText(string text)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      foreach (var keyEvent in text.Select(character => new Event {character = character}))
      {
        inputField.ProcessEvent(keyEvent);
      }

      inputField.ForceLabelUpdate();
    }

    /// <summary>
    /// Inputs the given input event into the currently selected text input field.
    /// </summary>
    /// <param name="inputEvent">Input event as event</param>
    public static void InputEvent(Event inputEvent)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      inputField.ProcessEvent(inputEvent);
      inputField.ForceLabelUpdate();
    }

    /// <summary>
    /// Inputs the given keyboard event into the currently selected text input field.
    /// </summary>
    /// <param name="eventString">Keyboard event string</param>
    public static void InputKeyboardEvent(string eventString)
    {
      InputEvent(Event.KeyboardEvent(eventString));
    }

    /// <summary>
    /// Inputs a backspace into the currently selected text input field.
    /// </summary>
    public static void InputBackspace()
    {
      InputKeyboardEvent("backspace");
    }

    /// <summary>
    /// Inputs a return into the currently selected text input field.
    /// </summary>
    public static void InputReturn()
    {
      InputKeyboardEvent('\n'.ToString());
    }

    /// <summary>
    /// Inputs a left arrow navigation event into the currently selected text input field.
    /// </summary>
    public static void InputLeftArrow()
    {
      InputKeyboardEvent("LeftArrow");
    }

    /// <summary>
    /// Inputs a right arrow navigation event into the currently selected text input field.
    /// </summary>
    public static void InputRightArrow()
    {
      InputKeyboardEvent("RightArrow");
    }

    /// <summary>
    /// Inputs a tab character into the currently selected text input field.
    /// </summary>
    public static void InputTabulator()
    {
      InputKeyboardEvent('\t'.ToString());
    }

    /// <summary>
    /// Retrieves the currently selected text input field.
    /// Returns null in case no text input field is currently selected.
    /// </summary>
    /// <returns>The currently selected text input field or null</returns>
    private static TMP_InputField GetSelectedInputField()
    {
      var selectedObject = EventSystem.current.currentSelectedGameObject;

      return selectedObject != null && selectedObject.TryGetComponent<TMP_InputField>(out var inputField)
        ? inputField
        : null;
    }
  }
}
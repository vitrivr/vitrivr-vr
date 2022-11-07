using System;
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
    /// Stores the last input field into which more than a single character has been input at once.
    /// Clears after any other interaction.
    /// </summary>
    private static TMP_InputField _lastLargeInputField;

    /// <summary>
    /// Stores the cursor position at the end of the last input if it was an input of more than a single character.
    /// </summary>
    private static int _lastLargeInputCursorPosition = -1;

    /// <summary>
    /// Inputs the given text into the currently selected text input field.
    /// </summary>
    /// <param name="text"></param>
    public static void InputText(string text)
    {
      // Check if input is more than a single character
      var largeInput = text.Length > 1;

      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      // Prepend space if input is word/phrase or last input was a word/phrase and this input is not a space
      if ((largeInput || (text != " " && LargeLastInput(inputField))) && SpaceNecessary(inputField))
      {
        text = " " + text;
      }

      foreach (var keyEvent in text.Select(character => new Event {character = character}))
      {
        inputField.ProcessEvent(keyEvent);
      }

      inputField.ForceLabelUpdate();

      // If input is larger than single character, store necessary information, otherwise reset
      _lastLargeInputField = largeInput ? inputField : null;
      _lastLargeInputCursorPosition = largeInput ? inputField.caretPosition : -1;
    }

    /// <summary>
    /// Inputs the given input events into the currently selected text input field.
    /// </summary>
    /// <param name="inputEvents">Input events as events</param>
    public static void InputEvent(params Event[] inputEvents)
    {
      var inputField = GetSelectedInputField();

      if (inputField == null)
      {
        return; // No text field selected, nothing to do
      }

      foreach (var inputEvent in inputEvents)
      {
        inputField.ProcessEvent(inputEvent);
      }

      inputField.ForceLabelUpdate();

      // Reset large input information
      _lastLargeInputField = null;
      _lastLargeInputCursorPosition = 0;
    }

    /// <summary>
    /// Inputs the given keyboard events into the currently selected text input field.
    /// </summary>
    /// <param name="eventStrings">Keyboard event strings</param>
    public static void InputKeyboardEvent(params string[] eventStrings)
    {
      InputEvent(eventStrings.Select(Event.KeyboardEvent).ToArray());
    }

    /// <summary>
    /// Inputs a backspace into the currently selected text input field.
    /// In case the last input was more than a single character, 
    /// </summary>
    public static void InputBackspace()
    {
      var inputField = GetSelectedInputField();

      if (LargeLastInput(inputField))
      {
        // Determine length of last word
        var wordLength = inputField.text[.._lastLargeInputCursorPosition].Split(" ").Last().Length;
        var events = new string[wordLength];
        Array.Fill(events, "backspace");
        InputKeyboardEvent(events);
      }
      else
      {
        InputKeyboardEvent("backspace");
      }
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

    /// <summary>
    /// Determines if a space is necessary to insert a new word or phrase into the given input field.
    /// This is the case if the cursor is not at the start of the input field and the previous character is not a space.
    /// </summary>
    private static bool SpaceNecessary(TMP_InputField inputField)
    {
      var cursor = inputField.caretPosition;
      return cursor != 0 && inputField.text.Substring(cursor - 1, 1) != " ";
    }

    /// <summary>
    /// Checks if the last input was more than a character and no inputs or selections have happened since then.
    /// </summary>
    private static bool LargeLastInput(TMP_InputField inputField)
    {
      var sameInputField = inputField == _lastLargeInputField;
      var samePosition = inputField.caretPosition == _lastLargeInputCursorPosition;
      var noSelection = inputField.selectionAnchorPosition == inputField.selectionFocusPosition;
      return sameInputField && samePosition && noSelection;
    }
  }
}
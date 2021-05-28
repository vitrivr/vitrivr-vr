using System.Linq;
using TMPro;
using UnityEngine;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Class to interface between an input field and the <see cref="TextInputManager"/>.
  ///
  /// Attach to the game object containing the text input.
  /// This class could be made more generic to support other types of text input.
  /// </summary>
  public class InputFieldController : MonoBehaviour
  {
    private TMP_InputField _inputField;

    public bool IsFocused => _inputField.isFocused;

    private void Start()
    {
      _inputField = GetComponent<TMP_InputField>();
      TextInputManager.Register(this);
    }

    private void OnDestroy()
    {
      TextInputManager.Unregister(this);
    }

    public void InputText(string text)
    {
      foreach (var keyEvent in text.Select(character => new Event {character = character}))
      {
        _inputField.ProcessEvent(keyEvent);
      }

      _inputField.ForceLabelUpdate();
    }

    public void InputBackspace()
    {
      _inputField.ProcessEvent(Event.KeyboardEvent("backspace"));
      _inputField.ForceLabelUpdate();
    }
  }
}
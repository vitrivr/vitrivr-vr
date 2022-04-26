using System;
using UnityEngine;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Class to interface between an input field and the <see cref="TextInputManager"/>.
  ///
  /// Attach to the game object containing the text input.
  /// This class could be made more generic to support other types of text input.
  /// </summary>
  [Obsolete("InputFieldController will be removed, TextInputManager can now find and access TextInputFields itself.")]
  public class InputFieldController : MonoBehaviour
  {
  }
}
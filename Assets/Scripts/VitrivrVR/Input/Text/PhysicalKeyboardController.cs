using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VitrivrVR.Input.Text
{
  public class PhysicalKeyboardController : MonoBehaviour
  {
    [Serializable]
    public class TextInputEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class TextBackspaceEvent : UnityEvent
    {
    }

    public PhysicalKeyboardKeyController keyboardKeyPrefab;
    public List<string> keys;
    public float keySize = .1f;
    public float keyPadding;
    public float spaceBarSize = .5f;
    public TextInputEvent onKeyPress;
    public TextBackspaceEvent onBackspace;

    private void Start()
    {
      GenerateKeyboard();
    }

    private void GenerateKeyboard()
    {
      if (!keyboardKeyPrefab)
      {
        Debug.LogError("No keyboard key prefab specified!");
        return;
      }

      var originY = (keys.Count - 1) * (keySize + keyPadding) / 2f;

      for (var y = 0; y < keys.Count; y++)
      {
        var rowDepth = originY - y * (keySize + keyPadding);
        var originX = (keys[y].Length - 1) * (keySize + keyPadding) / 2f;
        for (var x = 0; x < keys[y].Length; x++)
        {
          var keyWidth = keys[y][x] == ' ' ? spaceBarSize : keySize;
          var key = Instantiate(keyboardKeyPrefab, transform);
          // Displace and scale
          var transform1 = key.transform;
          transform1.localPosition = new Vector3(x * (keySize + keyPadding) - originX, 0, rowDepth);
          transform1.localScale = new Vector3(keyWidth, keySize, keySize);
          // Set up action
          var character = keys[y][x].ToString();
          if (character == "<")
          {
            key.onPress = BackspacePressed;
          }
          else
          {
            key.onPress = () => KeyPressed(character);
          }

          key.SetText(character);
        }
      }
    }

    private void KeyPressed(string text)
    {
      onKeyPress.Invoke(text);
    }

    private void BackspacePressed()
    {
      onBackspace.Invoke();
    }

    private void OnDrawGizmos()
    {
      Gizmos.color = Color.yellow;
      Gizmos.matrix = transform.localToWorldMatrix;

      var originY = (keys.Count - 1) * (keySize + keyPadding) / 2f;

      for (var y = 0; y < keys.Count; y++)
      {
        var rowDepth = originY - y * (keySize + keyPadding);
        var originX = (keys[y].Length - 1) * (keySize + keyPadding) / 2f;
        for (var x = 0; x < keys[y].Length; x++)
        {
          var keyWidth = keys[y][x] == ' ' ? spaceBarSize : keySize;
          Gizmos.DrawWireCube(new Vector3(x * (keySize + keyPadding) - originX, 0, rowDepth),
            new Vector3(keyWidth, keySize, keySize));
        }
      }
    }
  }
}
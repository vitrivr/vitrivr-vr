using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInputController : MonoBehaviour
{
    public TMP_InputField textField;
    public Button keyboardButtonPrefab;
    public Canvas keyboardCanvasPrefab;
    public string[] keys;

    private float _buttonSize;
    private Canvas _keyboard;

    void Awake()
    {
        var buttonRect = keyboardButtonPrefab.GetComponent<RectTransform>();
        _buttonSize = buttonRect.rect.height;
    }

    public void ShowKeyboard()
    {
        if (_keyboard != null)
        {
            Debug.LogError("Tried to open keyboard, but keyboard already open!");
            return;
        }

        _keyboard = Instantiate(keyboardCanvasPrefab, new Vector3(0, 1,-0.1f ), Quaternion.identity);
        
        _keyboard.worldCamera = Camera.main;

        var j = 0;
        foreach (var row in keys)
        {
            var i = 0;
            foreach (var key in row)
            {
                var button = Instantiate(keyboardButtonPrefab, _keyboard.transform);
                if (key == '<')
                {
                    button.onClick.AddListener(() =>
                    {
                        if (textField.text.Length > 0)
                        {
                            textField.text = textField.text.Substring(0, textField.text.Length - 1);
                        }
                    });
                }
                else
                {
                    button.onClick.AddListener(() => textField.text += key);
                }
                var buttonText = button.GetComponentInChildren<TMP_Text>();
                buttonText.text = key.ToString().ToUpper();
                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.anchoredPosition = new Vector2(i * _buttonSize, -j * _buttonSize);
                i++;
            }
            j++;
        }
    }

    public void HideKeyboard()
    {
        
    }
}

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
                            var caretPosition = textField.caretPosition;
                            if (caretPosition == 0)
                            {
                                caretPosition = textField.text.Length;
                            }
                            textField.text = textField.text.Remove(caretPosition - 1, 1);
                            textField.caretPosition = Mathf.Max(caretPosition - 1, 0);
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
        
        // Space bar
        var spaceKey = Instantiate(keyboardButtonPrefab, _keyboard.transform);
        spaceKey.onClick.AddListener(() => textField.text += ' ');
        var spaceKeyText = spaceKey.GetComponentInChildren<TMP_Text>();
        spaceKeyText.text = "_";
        var spaceKeyRect = spaceKey.GetComponent<RectTransform>();
        var spaceKeyWidth = _buttonSize * 4;
        spaceKeyRect.anchoredPosition = new Vector2(0, -j * _buttonSize);
        spaceKeyRect.sizeDelta = new Vector2(spaceKeyWidth, _buttonSize);
        
        // Hide key
        var hideKey = Instantiate(keyboardButtonPrefab, _keyboard.transform);
        hideKey.onClick.AddListener(HideKeyboard);
        var hideKeyText = hideKey.GetComponentInChildren<TMP_Text>();
        hideKeyText.text = "DONE";
        var hideKeyRect = hideKey.GetComponent<RectTransform>();
        var hideKeyWidth = _buttonSize * 4;
        hideKeyRect.anchoredPosition = new Vector2(spaceKeyWidth, -j * _buttonSize);
        hideKeyRect.sizeDelta = new Vector2(hideKeyWidth, _buttonSize);
    }

    public void HideKeyboard()
    {
        Destroy(_keyboard.gameObject);
        _keyboard = null;
    }
}

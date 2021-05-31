using System;
using TMPro;
using UnityEngine;

namespace VitrivrVR.Notification
{
  public class NotificationDisplay : MonoBehaviour
  {
    /// <summary>
    /// Time until notification text disappears.
    /// </summary>
    public float fadeTime = 10;

    private TextMeshPro _textDisplay;
    private float _fadeTimer;

    private void Awake()
    {
      _textDisplay = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
      if (!(_fadeTimer > 0)) return;
      
      _fadeTimer -= Time.deltaTime;
      if (_fadeTimer <= 0)
      {
        _textDisplay.text = "";
      }
    }

    public void SetNotification(string notification)
    {
      var time = DateTime.Now.ToString("T");
      _textDisplay.text = $"[{time}] {notification}";
      _fadeTimer = fadeTime;
    }
  }
}
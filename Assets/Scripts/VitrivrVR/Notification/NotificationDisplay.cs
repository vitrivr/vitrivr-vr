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

    private void Start()
    {
      _textDisplay = GetComponent<TextMeshPro>();
      NotificationController.Notify("Test Notification!");
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
      _textDisplay.text = notification;
      _fadeTimer = fadeTime;
    }
  }
}
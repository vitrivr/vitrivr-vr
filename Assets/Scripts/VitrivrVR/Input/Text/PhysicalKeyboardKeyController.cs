using System;
using TMPro;
using UnityEngine;

namespace VitrivrVR.Input.Text
{
  public class PhysicalKeyboardKeyController : MonoBehaviour
  {
    public Action onPress;

    private TextMeshPro _tmp;

    private void Awake()
    {
      _tmp = GetComponentInChildren<TextMeshPro>();
    }

    public void SetText(string text)
    {
      _tmp.text = text;
    }

    public void OnTriggerEnter(Collider other)
    {
      onPress();
    }
  }
}
using System;
using TMPro;
using UnityEngine;

namespace VitrivrVR.Input.Text
{
  public class PhysicalKeyboardKeyController : MonoBehaviour
  {
    public Color defaultColor;
    public Color pressedColor;

    public Action onPress;

    private TextMeshPro _tmp;
    private Renderer _renderer;

    private void Awake()
    {
      _tmp = GetComponentInChildren<TextMeshPro>();
      _renderer = GetComponent<Renderer>();
    }

    public void SetText(string text)
    {
      _tmp.text = text;
    }

    public void OnTriggerEnter(Collider other)
    {
      onPress();
      _renderer.material.color = pressedColor;
    }

    public void OnTriggerExit(Collider other)
    {
      _renderer.material.color = defaultColor;
    }
  }
}
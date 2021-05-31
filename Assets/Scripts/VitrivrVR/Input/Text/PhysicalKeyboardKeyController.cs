using System;
using TMPro;
using UnityEngine;
using VitrivrVR.Interaction.System;

namespace VitrivrVR.Input.Text
{
  public class PhysicalKeyboardKeyController : Interactable
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

    public override void OnHoverEnter(Transform interactor)
    {
      onPress();
      _renderer.material.color = pressedColor;
    }

    public override void OnHoverExit(Transform interactor)
    {
      _renderer.material.color = defaultColor;
    }
  }
}
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class SegmentInfoView : ViewerToolView
  {
    private TextMeshProUGUI _text;
    private Canvas _canvas;

    private void Start()
    {
      _canvas = GetComponentInChildren<Canvas>();
      _canvas.worldCamera = Camera.main;
      _text = GetComponentInChildren<TextMeshProUGUI>();
      _text.text = "No Segment Selected";
    }
  }
}
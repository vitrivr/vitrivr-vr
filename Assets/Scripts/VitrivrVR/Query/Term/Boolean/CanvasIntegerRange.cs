using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasIntegerRange : CanvasBooleanTerm
  {
    public GameObject sliders;
    public TextMeshProUGUI optionName;
    public TextMeshProUGUI slider0Text;
    public TextMeshProUGUI slider1Text;
    public Slider slider0;
    public Slider slider1;

    private int _value0, _value1;
    private bool _enabled;
    private string _entity;

    public void Initialize(string optionTitle, string entity, int min, int max)
    {
      _entity = entity;

      optionName.text = optionTitle;
      slider0.minValue = min;
      slider0.maxValue = max;
      slider1.minValue = min;
      slider1.maxValue = max;
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      return _value0 == _value1
        ? (_entity, RelationalOperator.Eq, new[] { _value0.ToString() })
        : (_entity, RelationalOperator.Between,
          new[] { Math.Min(_value0, _value1).ToString(), Math.Max(_value0, _value1).ToString() });
    }

    public override bool IsEnabled()
    {
      return _enabled;
    }

    public void SetEnabled(bool enable)
    {
      _enabled = enable;
      sliders.SetActive(enable);
    }

    public void SetSlider0(float n)
    {
      _value0 = (int)n;
      slider0Text.text = _value0.ToString();
    }

    public void SetSlider1(float n)
    {
      _value1 = (int)n;
      slider1Text.text = _value1.ToString();
    }
  }
}
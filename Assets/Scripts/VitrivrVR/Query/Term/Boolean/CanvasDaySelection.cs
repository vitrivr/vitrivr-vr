using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasDaySelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public Transform togglesParent;

    private Toggle[] _toggles;
    private string _attribute;
    private string[] _options;

    public void Initialize(string optionTitle, string attribute, List<string> availableDays)
    {
      optionName.text = optionTitle;
      _attribute = attribute;
      _options = availableDays.ToArray();

      _toggles = togglesParent.GetComponentsInChildren<Toggle>();

      if (_toggles.Length != 31)
      {
        Debug.LogError($"Expected 31 day toggles, but found {_toggles.Length}!");
      }

      foreach (var (day, toggle) in Enumerable.Range(1, 31).Select(i => i.ToString()).Zip(_toggles, Tuple.Create))
      {
        toggle.GetComponentInChildren<TMP_Text>().text = day;
        toggle.interactable = availableDays.Contains(day);
      }
    }

    public override List<(string attribute, RelationalOperator op, string[] values)> GetTerms()
    {
      if (!_toggles.Any(x => x.isOn))
      {
        Debug.LogError("Requested term from CanvasDaySelection despite no selection!");
        return new List<(string attribute, RelationalOperator op, string[] values)>
          { (null, RelationalOperator.Eq, null) };
      }

      var options = _toggles.Select(toggle => toggle.isOn)
        .Zip(_options, (use, option) => (use, option))
        .Where(value => value.use)
        .Select(value => value.option)
        .ToArray();

      return new List<(string attribute, RelationalOperator op, string[] values)>
      {
        options.Length == 1
          ? (_attribute, RelationalOperator.Eq, options)
          : (_attribute, RelationalOperator.In, options)
      };
    }

    public override bool IsEnabled()
    {
      return _toggles.Any(x => x.isOn);
    }
  }
}
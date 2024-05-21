using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasYearSelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public Transform togglesParent;
    public Toggle yearTogglePrefab;

    private readonly List<Toggle> _toggles = new();
    private string _attribute;
    private string[] _options;

    public void Initialize(string optionTitle, string attribute, List<string> availableYears)
    {
      optionName.text = optionTitle;
      _attribute = attribute;
      _options = availableYears.ToArray();

      foreach (var year in availableYears)
      {
        var toggle = Instantiate(yearTogglePrefab, togglesParent);
        _toggles.Add(toggle);
        toggle.GetComponentInChildren<TMP_Text>().text = year;
      }
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      if (!_toggles.Any(x => x.isOn))
      {
        Debug.LogError("Requested term from CanvasYearSelection despite no selection!");
        return (null, RelationalOperator.Eq, null);
      }

      var options = _toggles.Select(toggle => toggle.isOn)
        .Zip(_options, (use, option) => (use, option))
        .Where(value => value.use)
        .Select(value => value.option)
        .ToArray();

      return options.Length == 1
        ? (_attribute, RelationalOperator.Eq, options)
        : (_attribute, RelationalOperator.In, options);
    }

    public override bool IsEnabled()
    {
      return _toggles.Any(x => x.isOn);
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasWeekdaySelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public Toggle[] toggles;

    private string _attribute;
    private string[] _options;

    public void Initialize(string optionTitle, string attribute, string[] options)
    {
      optionName.text = optionTitle;
      _attribute = attribute;
      _options = options;
      if (_options.Length != 7)
      {
        Debug.LogWarning("More than 7 options provided to WeekdaySelection!");
      }
    }

    public override List<(string attribute, RelationalOperator op, string[] values)> GetTerms()
    {
      var selection = toggles.Select(toggle => toggle.isOn).ToArray();
      if (!selection.Any(x => x))
      {
        Debug.LogError("Requested term from CanvasWeekdaySelection despite no selection!");
        return new List<(string attribute, RelationalOperator op, string[] values)>
          { (null, RelationalOperator.Eq, null) };
      }

      var options = selection
        .Zip(_options, (use, option) => (use, option))
        .Where(value => value.use)
        .Select(value => int.TryParse(value.option, out _) ? value.option : "\"" + value.option + "\"")
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
      return toggles.Select(toggle => toggle.isOn).Any(x => x);
    }
  }
}
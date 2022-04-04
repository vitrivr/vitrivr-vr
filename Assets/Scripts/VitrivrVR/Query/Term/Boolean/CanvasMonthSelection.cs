using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasMonthSelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public Toggle[] toggles;

    private string _attribute;
    private string[] _options;

    /// <summary>
    /// Initialize this CanvasMonthSelection
    /// </summary>
    /// <param name="optionTitle">Display name of this Boolean selection.</param>
    /// <param name="attribute">Full name of the month database attribute (e.g. table.column).</param>
    /// <param name="monthIds">IDs of the months used internally in the database (in order).</param>
    /// <param name="availableMonths">Array containing the IDs for all available months.</param>
    public void Initialize(string optionTitle, string attribute, string[] monthIds, List<string> availableMonths)
    {
      optionName.text = optionTitle;
      _attribute = attribute;
      _options = monthIds;
      if (_options.Length != 12)
      {
        Debug.LogWarning("More than 12 options provided to MonthSelection!");
      }
      
      foreach (var (active, toggle) in monthIds.Select(availableMonths.Contains).Zip(toggles, Tuple.Create))
      {
        toggle.interactable = active;
      }
    }
    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      var selection = toggles.Select(toggle => toggle.isOn).ToArray();
      if (!selection.Any(x => x))
      {
        Debug.LogError("Requested term from CanvasMonthSelection despite no selection!");
        return (null, RelationalOperator.Eq, null);
      }

      var options = selection
        .Zip(_options, (use, option) => (use, option))
        .Where(value => value.use)
        .Select(value => "\"" + value.option + "\"")
        .ToArray();

      return options.Length == 1
        ? (_attribute, RelationalOperator.Eq, options)
        : (_attribute, RelationalOperator.In, options);
    }

    public override bool IsEnabled()
    {
      return toggles.Select(toggle => toggle.isOn).Any(x => x);
    }
  }
}
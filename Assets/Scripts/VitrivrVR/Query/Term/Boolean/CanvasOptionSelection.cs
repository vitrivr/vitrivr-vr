using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasOptionSelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public TMP_Dropdown operatorDropdown;
    public TMP_Dropdown valueDropdown;
    public GameObject dropdowns;

    private string _entity;
    private List<RelationalOperator> _operators;
    private List<string> _options;

    private bool _enabled;
    private bool _numeric;

    public void Initialize(string optionTitle, string entity, List<RelationalOperator> operators, List<string> options,
      bool numeric)
    {
      _entity = entity;
      _operators = operators;
      _options = options;
      // If numeric, do not add quotes around value for query term
      _numeric = numeric;

      optionName.text = optionTitle;
      operatorDropdown.AddOptions(_operators.Select(op => op.ToString()).ToList());
      valueDropdown.AddOptions(_options);
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      var value = _options[valueDropdown.value];
      if (!_numeric)
      {
        value = "\"" + value + "\"";
      }

      return (_entity, _operators[operatorDropdown.value], new[] { value });
    }

    public override bool IsEnabled()
    {
      return _enabled;
    }

    public void SetEnabled(bool enable)
    {
      _enabled = enable;
      dropdowns.SetActive(enable);
    }
  }
}
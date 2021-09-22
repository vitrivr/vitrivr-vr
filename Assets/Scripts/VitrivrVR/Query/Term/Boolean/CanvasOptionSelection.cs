using System.Collections.Generic;
using System.Linq;
using TMPro;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasOptionSelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public TMP_Dropdown operatorDropdown;
    public TMP_Dropdown valueDropdown;

    private string _entity;
    private List<RelationalOperator> _operators;
    private List<string> _options;

    public void Initialize(string optionTitle, string entity, List<RelationalOperator> operators, List<string> options)
    {
      _entity = entity;
      _operators = operators;
      _options = options;

      optionName.text = optionTitle;
      operatorDropdown.AddOptions(_operators.Select(op => op.ToString()).ToList());
      valueDropdown.AddOptions(_options);
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      // TODO: Default empty option to be ignored
      return (_entity, _operators[operatorDropdown.value], new[] { _options[valueDropdown.value] });
    }
  }
}
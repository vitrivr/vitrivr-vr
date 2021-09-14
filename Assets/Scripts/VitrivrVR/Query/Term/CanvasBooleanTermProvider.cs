using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using Vitrivr.UnityInterface.CineastApi.Model.Query;
using Vitrivr.UnityInterface.CineastApi.Utils;

namespace VitrivrVR.Query.Term
{
  public class CanvasBooleanTermProvider : QueryTermProvider
  {
    public TMP_Dropdown categoryDropdown;
    public TMP_Dropdown operatorDropdown;
    public TMP_InputField valueField;

    private string[] _categories = {
      "features_table_lsc20meta.timezone",
      "features_table_lsc20meta.p_day_of_week",
      "features_table_lsc20meta.p_hour"
    };
    private RelationalOperator[] _operators = {
      RelationalOperator.Eq,
      RelationalOperator.NEq
    };

    public override List<QueryTerm> GetTerms()
    {
      if (string.IsNullOrEmpty(valueField.text))
        return new List<QueryTerm>();

      var category = categoryDropdown.value;
      var op = operatorDropdown.value;

      return new List<QueryTerm>
      {
        QueryTermBuilder.BuildBooleanTerm(_categories[category], _operators[op], valueField.text)
      };
    }
  }
}
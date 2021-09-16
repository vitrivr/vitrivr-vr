using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Query;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;

namespace VitrivrVR.Query.Term
{
  public class CanvasBooleanTermProvider : QueryTermProvider
  {
    public TMP_Dropdown categoryDropdown;
    public TMP_Dropdown operatorDropdown;
    public TMP_Dropdown valueDropdown;

    private enum BooleanTermTypes
    {
      IntegerRange,
      Options,
      DynamicOptions
    }

    private List<(string entity, List<RelationalOperator> ops, List<string> values)> _categories =
      new List<(string entity, List<RelationalOperator> ops, List<string> values)>();

    private async void Start()
    {
      foreach (var category in ConfigManager.Config.booleanCategories)
      {
        if (Enum.TryParse<BooleanTermTypes>(category.selectionType, out var termType))
        {
          switch (termType)
          {
            case BooleanTermTypes.IntegerRange:
              CreateIntegerRange(category);
              break;
            case BooleanTermTypes.Options:
              CreateOptions(category);
              break;
            case BooleanTermTypes.DynamicOptions:
              await CreateDynamicOptions(category);
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
        else
        {
          Debug.LogError($"Unknown Boolean term type: {category.selectionType}");
        }
      }
      
      SelectCategory(0);
    }

    private void CreateIntegerRange(VitrivrVrConfig.BooleanCategory category)
    {
      var entity = $"{category.table}.{category.attribute}";
      var ops = new List<RelationalOperator> { RelationalOperator.Eq };
      var start = int.Parse(category.options[0]);
      var end = int.Parse(category.options[1]);
      var values = Enumerable.Range(start, end - start).Select(i => i.ToString()).ToList();
      _categories.Add((entity, ops, values));

      categoryDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData(category.name) });
    }

    private void CreateOptions(VitrivrVrConfig.BooleanCategory category)
    {
      var entity = $"{category.table}.{category.attribute}";
      var ops = new List<RelationalOperator> { RelationalOperator.Eq };
      _categories.Add((entity, ops, category.options.ToList()));

      categoryDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData(category.name) });
    }

    private async Task CreateDynamicOptions(VitrivrVrConfig.BooleanCategory category)
    {
      var entity = $"{category.table}.{category.attribute}";
      var ops = new List<RelationalOperator> { RelationalOperator.Eq };
      var options = await CineastWrapper.GetDistinctTableValues(category.table, category.attribute);
      _categories.Add((entity, ops, options));

      categoryDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData(category.name) });
    }

    public void SelectCategory(int index)
    {
      operatorDropdown.ClearOptions();
      operatorDropdown.AddOptions(_categories[index].ops.Select(op => op.ToString()).ToList());
      
      valueDropdown.ClearOptions();
      valueDropdown.AddOptions(_categories[index].values);
    }

    public override List<QueryTerm> GetTerms()
    {
      var (entity, ops, values) = _categories[categoryDropdown.value];
      var op = ops[operatorDropdown.value];
      var value = values[valueDropdown.value];

      return new List<QueryTerm>
      {
        QueryTermBuilder.BuildBooleanTerm(entity, op, value)
      };
    }
  }
}
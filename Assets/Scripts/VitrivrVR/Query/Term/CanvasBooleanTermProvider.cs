using System;
using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Query;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;
using VitrivrVR.Query.Term.Boolean;

namespace VitrivrVR.Query.Term
{
  public class CanvasBooleanTermProvider : QueryTermProvider
  {
    public CanvasWeekdaySelection weekdaySelection;
    public CanvasOptionSelection optionSelection;

    private enum BooleanTermTypes
    {
      IntegerRange,
      WeekdayOptions,
      DynamicOptions
    }

    private List<CanvasBooleanTerm> _termProviders = new List<CanvasBooleanTerm>();

    private async void Start()
    {
      var categories = ConfigManager.Config.booleanCategories;

      if (categories.Count == 0)
      {
        Debug.LogError("Boolean term enabled with no Boolean categories specified!");
        Destroy(transform.root.gameObject);
        return;
      }

      foreach (var category in categories)
      {
        if (Enum.TryParse<BooleanTermTypes>(category.selectionType, out var termType))
        {
          switch (termType)
          {
            case BooleanTermTypes.IntegerRange:
              break;
            case BooleanTermTypes.WeekdayOptions:
              var weekdayOptions = Instantiate(weekdaySelection, transform);
              weekdayOptions.Initialize($"{category.table}.{category.column}", category.options);
              _termProviders.Add(weekdayOptions);
              break;
            case BooleanTermTypes.DynamicOptions:
              var dynamicOptions = Instantiate(optionSelection, transform);
              var dynOpt = await CineastWrapper.GetDistinctTableValues(category.table, category.column);
              dynamicOptions.Initialize(category.name, $"{category.table}.{category.column}",
                new List<RelationalOperator>
                {
                  RelationalOperator.Eq,
                  RelationalOperator.NEq
                }, dynOpt);
              _termProviders.Add(dynamicOptions);
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
    }

    public override List<QueryTerm> GetTerms()
    {
      var termParts = _termProviders
        .Select(provider => provider.GetTerm())
        .Where(t => t.attribute != null)
        .ToArray();

      return termParts.Length == 0
        ? new List<QueryTerm>()
        : new List<QueryTerm>
        {
          QueryTermBuilder.BuildBooleanTerm(termParts)
        };
    }
  }
}
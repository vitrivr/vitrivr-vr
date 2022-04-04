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
    public CanvasMonthSelection monthSelection;
    public CanvasOptionSelection optionSelection;
    public CanvasIntegerRange integerRange;

    private enum BooleanTermTypes
    {
      IntegerRange,
      WeekdayOptions,
      MonthOptions,
      DynamicOptions
    }

    private enum SortOrder
    {
      Alphabetic,
      Numeric
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
          var entity = $"{category.table}.{category.column}";
          switch (termType)
          {
            case BooleanTermTypes.IntegerRange:
              var intRange = Instantiate(integerRange, transform);
              intRange.Initialize(category.name, entity, int.Parse(category.options[0]),
                int.Parse(category.options[1]));
              _termProviders.Add(intRange);
              break;
            case BooleanTermTypes.WeekdayOptions:
              var weekdayOptions = Instantiate(weekdaySelection, transform);
              weekdayOptions.Initialize(category.name, entity, category.options);
              _termProviders.Add(weekdayOptions);
              break;
            case BooleanTermTypes.MonthOptions:
              var monthOptions = Instantiate(monthSelection, transform);
              var availableMonths = await CineastWrapper.GetDistinctTableValues(category.table, category.column);
              monthOptions.Initialize(category.name, entity, category.options, availableMonths);
              _termProviders.Add(monthOptions);
              break;
            case BooleanTermTypes.DynamicOptions:
              var dynamicOptions = Instantiate(optionSelection, transform);
              var dynOpt = await CineastWrapper.GetDistinctTableValues(category.table, category.column);
              var numeric = false;
              if (category.options != null)
              {
                // TODO: Handle empty options array
                var sortOrder = category.options.First();
                dynOpt = SortOptions(dynOpt, sortOrder);
                numeric = Enum.TryParse<SortOrder>(sortOrder, out var order) && order == SortOrder.Numeric;
              }

              dynamicOptions.Initialize(category.name, entity,
                new List<RelationalOperator>
                {
                  RelationalOperator.Eq,
                  RelationalOperator.NEq
                }, dynOpt, numeric);
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
        .Where(t => t.IsEnabled())
        .Select(provider => provider.GetTerm())
        .ToArray();

      return termParts.Length == 0
        ? new List<QueryTerm>()
        : new List<QueryTerm>
        {
          QueryTermBuilder.BuildBooleanTerm(termParts)
        };
    }

    private static List<string> SortOptions(List<string> options, string sortOrder)
    {
      if (Enum.TryParse<SortOrder>(sortOrder, out var order))
      {
        switch (order)
        {
          case SortOrder.Alphabetic:
            options.Sort();
            return options;
          case SortOrder.Numeric:
            var numericSorted = options.Select(int.Parse).ToList();
            numericSorted.Sort();
            return numericSorted.Select(n => n.ToString()).ToList();
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      Debug.LogError($"Unknown sort order {sortOrder} specified!");
      return options;
    }
  }
}
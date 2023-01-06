using System;
using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;
using VitrivrVR.Query.Term.Boolean;

namespace VitrivrVR.Query.Term
{
  public class CanvasBooleanTermProvider : QueryTermProvider
  {
    public CanvasWeekdaySelection weekdaySelection;
    public CanvasDaySelection daySelection;
    public CanvasMonthSelection monthSelection;
    public CanvasYearSelection yearSelection;
    public CanvasOptionSelection optionSelection;
    public CanvasIntegerRange integerRange;
    
    public TMP_Text nameDisplayText;

    private enum BooleanTermTypes
    {
      IntegerRange,
      WeekdayOptions,
      DayOptions,
      MonthOptions,
      YearOptions,
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
            case BooleanTermTypes.DayOptions:
              var dayOptions = Instantiate(daySelection, transform);
              var availableDays =
                await QueryController.Instance.GetDistinctTableValues(category.table, category.column);
              dayOptions.Initialize(category.name, entity, availableDays);
              _termProviders.Add(dayOptions);
              break;
            case BooleanTermTypes.MonthOptions:
              var monthOptions = Instantiate(monthSelection, transform);
              var availableMonths =
                await QueryController.Instance.GetDistinctTableValues(category.table, category.column);
              monthOptions.Initialize(category.name, entity, category.options, availableMonths);
              _termProviders.Add(monthOptions);
              break;
            case BooleanTermTypes.YearOptions:
              var yearOptions = Instantiate(yearSelection, transform);
              var years = await QueryController.Instance.GetDistinctTableValues(category.table, category.column);
              years = SortOptions(years, SortOrder.Numeric);
              yearOptions.Initialize(category.name, entity, years);
              _termProviders.Add(yearOptions);
              break;
            case BooleanTermTypes.DynamicOptions:
              var dynamicOptions = Instantiate(optionSelection, transform);
              var dynOpt = await QueryController.Instance.GetDistinctTableValues(category.table, category.column);
              var numeric = false;
              if (category.options != null)
              {
                // TODO: Handle empty options array
                var sortOrder = category.options.First();
                dynOpt = SortOptions(dynOpt, sortOrder);
                numeric = Enum.TryParse<SortOrder>(sortOrder, out var order) && order == SortOrder.Numeric;
              }

              dynamicOptions.Initialize(category.name, entity, dynOpt, numeric);
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

    public override string GetTypeName()
    {
      return "Boolean";
    }
    
    public override void SetInstanceName(string displayName)
    {
      if (nameDisplayText != null)
      {
        nameDisplayText.text = displayName;
      }
    }

    private static List<string> SortOptions(List<string> options, SortOrder sortOrder)
    {
      switch (sortOrder)
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

    private static List<string> SortOptions(List<string> options, string sortOrder)
    {
      if (Enum.TryParse<SortOrder>(sortOrder, out var order))
      {
        return SortOptions(options, order);
      }

      Debug.LogError($"Unknown sort order {sortOrder} specified!");
      return options;
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasCalendarSelection : CanvasBooleanTerm
  {
    public Toggle togglePrefab;
    public TMP_Text optionName;
    public Transform togglesParent;
    public TMP_Text monthText;
    public Button previousButton;
    public Button nextButton;

    private List<DateTime> _days;
    private DateTime _currentMonth;
    private HashSet<DateTime> _selectedDays = new();
    private string _category;

    public void Initialize(string optionTitle, List<DateTime> days, string category)
    {
      optionName.text = optionTitle;
      _category = category;
      _days = days;
      _days.Sort();

      var firstMonth = GetMonth(_days.First());
      _currentMonth = firstMonth.First();
      CreateMonth(firstMonth);
    }

    public void NextMonth()
    {
      var nextMonth = GetMonth(GetFirstOfNextMonth(_currentMonth));
      _currentMonth = nextMonth.First();
      CreateMonth(nextMonth);
    }

    public void PreviousMonth()
    {
      var previousMonth = GetMonth(GetLastOfPreviousMonth(_currentMonth));
      _currentMonth = previousMonth.First();
      CreateMonth(previousMonth);
    }

    /// <summary>
    /// Returns all days of the given month in sorted order.
    /// </summary>
    /// <param name="month">DateTime object specifying the year and month to select by.</param>
    /// <returns>All days of the given month in sorted order</returns>
    private List<DateTime> GetMonth(DateTime month)
    {
      return _days.Where(day => day.Month == month.Month && day.Year == month.Year).ToList();
    }

    private DateTime GetFirstOfNextMonth(DateTime month)
    {
      return _days.FirstOrDefault(day => (day.Month > month.Month && day.Year == month.Year) || day.Year > month.Year);
    }

    private DateTime GetLastOfPreviousMonth(DateTime month)
    {
      return _days.LastOrDefault(day => (day.Month < month.Month && day.Year == month.Year) || day.Year < month.Year);
    }

    private void ClearToggles()
    {
      foreach (Transform child in togglesParent)
      {
        Destroy(child.gameObject);
      }
    }

    private void CreateMonth(List<DateTime> days)
    {
      ClearToggles();

      var day = new DateTime(days.First().Year, days.First().Month, 1, 0, 0, 0, DateTimeKind.Utc);

      // Determine the starting weekday of the month (shift by 1 for Monday = 0)
      var startingWeekday = ((int)day.DayOfWeek + 6) % 7;

      // Insert empty toggles for the days before the first day of the month
      for (var i = 0; i < startingWeekday; i++)
      {
        var empty = new GameObject();
        var rectTransform = empty.AddComponent<RectTransform>();
        rectTransform.SetParent(togglesParent);
      }

      while (day.Month == days.First().Month)
      {
        var toggle = Instantiate(togglePrefab, togglesParent);
        toggle.GetComponentInChildren<TMP_Text>().text = day.Day.ToString();
        if (days.Contains(day))
        {
          var d = day;
          toggle.onValueChanged.AddListener(isOn =>
          {
            if (isOn)
            {
              _selectedDays.Add(d);
            }
            else
            {
              _selectedDays.Remove(d);
            }
          });
        }
        else
        {
          toggle.interactable = false;
        }

        if (_selectedDays.Contains(day))
        {
          toggle.isOn = true;
        }

        day = day.AddDays(1);
      }

      monthText.text = $"{days.First():MMMM yyyy}";
      previousButton.interactable = _days.First() < days.First();
      nextButton.interactable = _days.Last() > days.Last();
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      if (_selectedDays.Count == 0)
      {
        Debug.LogError("Requested term from CanvasCalendarSelection despite no selection!");
        return (null, RelationalOperator.Eq, null);
      }

      var dates = _selectedDays.Select(date => ((DateTimeOffset)date).ToUnixTimeSeconds()).ToList();

      return (_category, RelationalOperator.In, dates.Select(date => date.ToString()).ToArray());
    }

    public override bool IsEnabled()
    {
      return _selectedDays.Count > 0;
    }
  }
}
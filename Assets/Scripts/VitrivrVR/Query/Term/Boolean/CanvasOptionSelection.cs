using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public class CanvasOptionSelection : CanvasBooleanTerm
  {
    public TMP_Text optionName;
    public Transform optionButtons;
    public Transform selectedButtons;
    public Button buttonPrefab;

    private const int Suggestions = 3;

    private string _entity;
    private List<string> _options;

    private bool _numeric;

    public void Initialize(string optionTitle, string entity, List<string> options,
      bool numeric)
    {
      _entity = entity;
      _options = options;
      // If numeric, do not add quotes around value for query term
      _numeric = numeric;

      optionName.text = optionTitle;
    }

    public void SearchTextChange(string text)
    {
      foreach (Transform option in optionButtons)
      {
        Destroy(option.gameObject);
      }

      if (text.Length == 0)
      {
        return;
      }

      var matching = _options.Where(value => value.Contains(text)).OrderBy(value => value.Length).Take(Suggestions);
      foreach (var value in matching)
      {
        var button = Instantiate(buttonPrefab, optionButtons);
        button.GetComponentInChildren<TMP_Text>().text = value;
        button.onClick.AddListener(() =>
        {
          var selectionButton = Instantiate(buttonPrefab, selectedButtons);
          selectionButton.GetComponentInChildren<TMP_Text>().text = value;
          selectionButton.onClick.AddListener(() => Destroy(selectionButton.gameObject));
        });
      }
    }

    public override (string attribute, RelationalOperator op, string[] values) GetTerm()
    {
      var selections = selectedButtons.GetComponentsInChildren<TMP_Text>().Select(text => text.text).ToArray();
      if (!_numeric)
      {
        selections = selections.Select(value => "\"" + value + "\"").ToArray();
      }

      return selections.Length == 1
        ? (_entity, RelationalOperator.Eq, selections)
        : (_entity, RelationalOperator.In, selections);
    }

    public override bool IsEnabled()
    {
      return selectedButtons.childCount > 0;
    }

    public override void Clear()
    {
      foreach (Transform button in selectedButtons)
      {
        Destroy(button.gameObject);
      }
    }
  }
}
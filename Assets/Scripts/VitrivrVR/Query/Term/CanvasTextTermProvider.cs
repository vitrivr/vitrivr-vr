using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine.UI;
using VitrivrVR.Config;

namespace VitrivrVR.Query.Term
{
  public class CanvasTextTermProvider : QueryTermProvider
  {
    public Toggle textTogglePrefab;

    // Text input data
    private string _textSearchText;
    private List<(string id, Toggle toggle)> _categories;

    private void Start()
    {
      var categories = ConfigManager.Config.textCategories;
      _categories = categories.Select((category, index) =>
      {
        var toggle = Instantiate(textTogglePrefab, transform);
        toggle.transform.SetSiblingIndex(index);
        var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
        text.text = category.name;
        return (category.id, toggle);
      }).ToList();
    }

    public void SetTextSearchText(string text)
    {
      _textSearchText = text;
    }

    public override List<QueryTerm> GetTerms()
    {
      var terms = new List<QueryTerm>();

      if (string.IsNullOrEmpty(_textSearchText))
        return terms;

      var categories = _categories
        .Where(category => category.toggle.isOn)
        .Select(category => category.id).ToList();

      if (categories.Count == 0)
        return terms;

      terms.Add(new QueryTerm(QueryTerm.TypeEnum.TEXT, _textSearchText, categories));

      return terms;
    }

    public override string GetTypeName()
    {
      return "Text";
    }
  }
}
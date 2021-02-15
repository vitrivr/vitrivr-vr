using System.Collections.Generic;
using System.Linq;
using System.Text;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VitrivrVR.Query;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class QueryListView : ViewerToolView
  {
    public RectTransform textPrefab;
    public Button buttonPrefab;
    public Transform list;

    private readonly List<RectTransform> _queries = new List<RectTransform>();

    private void Start()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      QueryController.Instance.queryAddedEvent.AddListener(OnQueryAdded);
      QueryController.Instance.queryRemovedEvent.AddListener(OnQueryRemoved);
      QueryController.Instance.queryFocusEvent.AddListener(OnQueryFocus);
      Initialize();
    }

    private void OnQueryAdded(int index)
    {
      AddQuery(QueryController.Instance.queries[index].query);
    }

    private void OnQueryRemoved(int index)
    {
      Destroy(_queries[index].gameObject);
    }

    private void OnQueryFocus(int oldIndex, int newIndex)
    {
      if (oldIndex != -1)
      {
        DeselectQuery(oldIndex);
      }

      if (newIndex != -1)
      {
        SelectQuery(newIndex);
      }
    }

    private void Initialize()
    {
      var queries = QueryController.Instance.queries;
      if (queries.Count == 0)
      {
        var textRect = Instantiate(textPrefab, list);
        var tmp = textRect.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = "No queries recorded.";
        textRect.sizeDelta = new Vector2(tmp.GetPreferredValues().x, textRect.sizeDelta.y);
      }
      else
      {
        foreach (var (query, _) in QueryController.Instance.queries)
        {
          AddQuery(query);
        }

        if (QueryController.Instance.CurrentQuery != -1)
        {
          SelectQuery(QueryController.Instance.CurrentQuery);
        }
      }
    }

    private void AddQuery(SimilarityQuery query)
    {
      const int padding = 20;
      if (_queries.Count == 0 && list.childCount > 0)
      {
        Destroy(list.GetChild(0).gameObject);
      }

      var button = Instantiate(buttonPrefab, list);
      // Set selected and highlighted color
      var colors = button.colors;
      colors.selectedColor = new Color(1f, .4f, .2f);
      colors.highlightedColor = new Color(1f, .7f, .5f);
      button.colors = colors;

      var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = QueryToString(query);
      var rect = button.GetComponent<RectTransform>();
      rect.sizeDelta = new Vector2(tmp.GetPreferredValues().x + padding, rect.sizeDelta.y);
      button.onClick.AddListener(() => QueryController.Instance.SelectQuery(query));
      _queries.Add(rect);
    }

    private void DeselectQuery(int index)
    {
      var button = _queries[index].GetComponent<Button>();
      var colors = button.colors;
      colors.normalColor = Color.white;
      button.colors = colors;
    }

    private void SelectQuery(int index)
    {
      var button = _queries[index].GetComponent<Button>();
      button.Select();
      var colors = button.colors;
      colors.normalColor = new Color(1f, .5f, .3f);
      button.colors = colors;
    }

    private static string QueryToString(SimilarityQuery query)
    {
      var stringBuilder = new StringBuilder();
      foreach (var component in query.Containers)
      {
        stringBuilder.Append("{");
        stringBuilder.Append(string.Join(", ", component.Terms.Select(TermToString)));
        stringBuilder.Append("}");
      }

      return stringBuilder.ToString();
    }

    private static string TermToString(QueryTerm term)
    {
      var categories = string.Join(", ", term.Categories);
      var baseString = $"{term.Type} ({categories})";
      switch (term.Type)
      {
        case QueryTerm.TypeEnum.IMAGE:
          return baseString;
        case QueryTerm.TypeEnum.TAG:
          var data = Base64Converter.StringFromBase64(term.Data.Substring(Base64Converter.JsonPrefix.Length));
          return $"{baseString}: {data}";
        default:
          return $"{baseString}: {term.Data}";
      }
    }
  }
}
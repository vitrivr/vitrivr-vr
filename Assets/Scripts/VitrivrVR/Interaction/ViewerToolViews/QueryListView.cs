using System.Collections.Generic;
using System.Linq;
using System.Text;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using VitrivrVR.Query;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class QueryListView : ViewerToolView
  {
    public RectTransform textPrefab;
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

    private void OnQueryFocus(int index)
    {
      // TODO: Query focus
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
      }
    }

    private void AddQuery(SimilarityQuery query)
    {
      if (_queries.Count == 0 && list.childCount > 0)
      {
        Destroy(list.GetChild(0).gameObject);
      }

      var textRect = Instantiate(textPrefab, list);
      var tmp = textRect.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = QueryToString(query);
      textRect.sizeDelta = new Vector2(tmp.GetPreferredValues().x, textRect.sizeDelta.y);
      _queries.Add(textRect);
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
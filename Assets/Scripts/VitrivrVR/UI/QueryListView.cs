using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VitrivrVR.Query;
using VitrivrVR.Query.Display;

namespace VitrivrVR.UI
{
  public class QueryListView : MonoBehaviour
  {
    public RectTransform textPrefab;
    public RectTransform listItemPrefab;
    public Transform list;

    private readonly List<RectTransform> _queries = new();

    private void Start()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      QueryController.Instance.queryAddedEvent.AddListener(OnQueryAdded);
      QueryController.Instance.queryRemovedEvent.AddListener(OnQueryRemoved);
      QueryController.Instance.queryFocusEvent.AddListener(OnQueryFocus);
      Initialize();
    }

    /// <summary>
    /// Removes all queries form the query controller.
    /// </summary>
    public void ClearAll()
    {
      QueryController.Instance.RemoveAllQueries();
    }

    private void OnQueryAdded(int index)
    {
      var display = QueryController.Instance.queries[index];
      AddQuery(display);
    }

    private void OnQueryRemoved(int index)
    {
      var query = _queries[index].gameObject;
      _queries.RemoveAt(index);
      Destroy(query);

      if (_queries.Count == 0)
      {
        AddQueriesEmptyText();
      }
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
        AddQueriesEmptyText();
      }
      else
      {
        foreach (var display in QueryController.Instance.queries)
        {
          AddQuery(display);
        }

        if (QueryController.Instance.CurrentQuery != -1)
        {
          SelectQuery(QueryController.Instance.CurrentQuery);
        }
      }
    }

    /// <summary>
    /// Adds the text indication that there are currently no queries in the history.
    /// </summary>
    private void AddQueriesEmptyText()
    {
      var textRect = Instantiate(textPrefab, list);
      var tmp = textRect.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = "No queries recorded.";
      textRect.sizeDelta = new Vector2(tmp.GetPreferredValues().x, textRect.sizeDelta.y);
    }

    private void AddQuery(QueryDisplay display)
    {
      const int padding = 20;
      if (_queries.Count == 0 && list.childCount > 0)
      {
        Destroy(list.GetChild(0).gameObject);
      }

      var listItem = Instantiate(listItemPrefab, list);

      // Prepare text button
      var textButton = listItem.GetChild(0).GetComponent<Button>();
      // Set selected and highlighted color
      var colors = textButton.colors;
      colors.selectedColor = new Color(1f, .4f, .2f);
      colors.highlightedColor = new Color(1f, .7f, .5f);
      textButton.colors = colors;

      var tmp = textButton.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = $"[{display.GetType().Name}] {display.GetQueryStringRepresentation()}";
      var textRect = textButton.GetComponent<RectTransform>();
      textRect.sizeDelta = new Vector2(tmp.GetPreferredValues().x + padding, textRect.sizeDelta.y);
      textButton.onClick.AddListener(() => QueryController.Instance.SelectQuery(display));

      // Prepare close button
      var closeButton = listItem.GetChild(1).GetComponent<Button>();
      closeButton.onClick.AddListener(() => QueryController.Instance.RemoveQuery(display));
      var closeRect = closeButton.GetComponent<RectTransform>();

      listItem.sizeDelta = new Vector2(textRect.sizeDelta.x + closeRect.sizeDelta.x, listItem.sizeDelta.y);
      _queries.Add(listItem);
    }

    private void DeselectQuery(int index)
    {
      var button = _queries[index].GetChild(0).GetComponent<Button>();
      var colors = button.colors;
      colors.normalColor = Color.white;
      button.colors = colors;
    }

    private void SelectQuery(int index)
    {
      var button = _queries[index].GetChild(0).GetComponent<Button>();
      button.Select();
      var colors = button.colors;
      colors.normalColor = new Color(1f, .5f, .3f);
      button.colors = colors;
    }
  }
}
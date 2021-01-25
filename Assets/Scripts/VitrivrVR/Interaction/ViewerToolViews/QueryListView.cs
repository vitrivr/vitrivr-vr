using TMPro;
using UnityEngine;
using VitrivrVR.Query;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class QueryListView : ViewerToolView
  {
    public RectTransform textPrefab;
    public Transform list;

    private void Start()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void OnEnable()
    {
      var queries = QueryController.Instance.queries;
      if (queries.Count == 0)
      {
        var textRect = Instantiate(textPrefab, list);
        var tmp = textRect.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = "No queries recorded.";
        textRect.sizeDelta = tmp.GetPreferredValues();
      }
      else
      {
        foreach (var (query, _) in QueryController.Instance.queries)
        {
          var textRect = Instantiate(textPrefab, list);
          var tmp = textRect.GetComponentInChildren<TextMeshProUGUI>();
          // TODO: Set query text appropriately
          tmp.text = query.ToString().Replace("\n", "");
          textRect.sizeDelta = new Vector2(tmp.GetPreferredValues().x, textRect.sizeDelta.y);
        }
      }
    }

    private void OnDisable()
    {
      foreach (Transform child in list)
      {
        Destroy(child.gameObject);
      }
    }
  }
}
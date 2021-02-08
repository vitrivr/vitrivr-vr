using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VitrivrVR.Data
{
  public class UITableController : MonoBehaviour
  {
    public string[,] table;
    public float minimumColumnWidth = 50;
    public float columnPadding = 20;
    public GameObject cellPrefab;

    private void Start()
    {
      if (table != null)
      {
        Initialize();
      }
    }

    public void Initialize()
    {
      var columnGameObject = new GameObject("Column", typeof(VerticalLayoutGroup));
      var layoutGroup = columnGameObject.GetComponent<VerticalLayoutGroup>();
      layoutGroup.childControlHeight = false;

      var cellHeight = cellPrefab.GetComponent<RectTransform>().sizeDelta.y;

      for (var i = 0; i < table.GetLength(1); i++)
      {
        var column = Instantiate(columnGameObject, transform);
        var columnWidth = minimumColumnWidth;
        for (var j = 0; j < table.GetLength(0); j++)
        {
          var cell = Instantiate(cellPrefab, column.transform);
          var tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
          tmp.text = table[j, i];
          columnWidth = Mathf.Max(columnWidth, tmp.GetPreferredValues().x + columnPadding);
        }

        var rectTransform = column.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(columnWidth, table.GetLength(1) * cellHeight);
      }
    }
  }
}
using TMPro;
using UnityEngine;

namespace VitrivrVR.UI
{
  public class UITableController : MonoBehaviour
  {
    public string[,] table;
    public float minimumColumnWidth = 50;
    public float columnPadding = 20;
    public GameObject cellPrefab;
    public GameObject columnPrefab;

    private float _cellHeight;
    private int _nRows;
    private int _nColumns;
    private RectTransform[] _columns;
    private GameObject[,] _cells;

    private void Start()
    {
      if (table != null)
      {
        Initialize();
      }
    }

    public void Initialize()
    {
      _nRows = table.GetLength(0);
      _nColumns = table.GetLength(1);

      _cellHeight = cellPrefab.GetComponent<RectTransform>().sizeDelta.y;
      _columns = new RectTransform[_nColumns];
      _cells = new GameObject[_nRows, _nColumns];
      
      var totalWidth = 0f;

      for (var i = 0; i < _nColumns; i++)
      {
        var column = Instantiate(columnPrefab, transform);
        var columnWidth = minimumColumnWidth;
        for (var j = 0; j < _nRows; j++)
        {
          var cell = Instantiate(cellPrefab, column.transform);
          var tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
          tmp.text = table[j, i];
          columnWidth = Mathf.Max(columnWidth, tmp.GetPreferredValues().x + columnPadding);
          _cells[j, i] = cell;
        }

        var rectTransform = column.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(columnWidth, _nRows * _cellHeight);
        _columns[i] = rectTransform;

        totalWidth += columnWidth;
      }

      var t = GetComponent<RectTransform>();
      t.sizeDelta = new Vector2(totalWidth, _nRows * _cellHeight);
    }

    public void SetCell(int row, int column, string text)
    {
      if (row < 0 || row >= _nRows || column < 0 || column >= _nColumns)
      {
        Debug.LogError($"Index ({row}, {column}) out of bounds for table of size ({_nRows}, {_nColumns})!");
        return;
      }

      var cell = _cells[row, column];
      var tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
      tmp.text = text;
      table[row, column] = text;
      var columnWidth = Mathf.Max(_columns[column].sizeDelta.x, tmp.GetPreferredValues().x + columnPadding);
      _columns[column].sizeDelta = new Vector2(columnWidth, table.GetLength(1) * _cellHeight);
    }
  }
}
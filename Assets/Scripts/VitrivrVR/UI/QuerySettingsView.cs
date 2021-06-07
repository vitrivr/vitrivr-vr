using UnityEngine;
using VitrivrVR.Query;
using VitrivrVR.Query.Display;

namespace VitrivrVR.UI
{
  public class QuerySettingsView : MonoBehaviour
  {
    public UITableController statisticsTable;

    private void Awake()
    {
      QueryController.Instance.queryFocusEvent.AddListener(OnQueryFocus);

      var currentQuery = QueryController.Instance.CurrentQuery;
      var results = currentQuery == -1
        ? "-----"
        : QueryController.Instance.queries[currentQuery].display.NumberOfResults.ToString();

      statisticsTable.table = new[,]
      {
        {"Results", results}
      };
    }

    public void SetObjectQueryDisplayMode()
    {
      SetQueryDisplayMode(QueryDisplay.DisplayMode.MediaObjectDisplay);
    }

    public void SetSegmentQueryDisplayMode()
    {
      SetQueryDisplayMode(QueryDisplay.DisplayMode.MediaSegmentDisplay);
    }

    private void SetQueryDisplayMode(QueryDisplay.DisplayMode displayMode)
    {
      var currentQuery = QueryController.Instance.CurrentQuery;
      if (currentQuery != -1)
      {
        QueryController.Instance.queries[currentQuery].display.SwitchDisplayMode(displayMode);
      }
    }

    private void OnQueryFocus(int oldIndex, int newIndex)
    {
      UpdateQueryStatistics(newIndex);
    }

    private void UpdateQueryStatistics(int queryIndex)
    {
      var results = queryIndex == -1
        ? "-----"
        : QueryController.Instance.queries[queryIndex].display.NumberOfResults.ToString();

      statisticsTable.SetCell(0, 1, results);
    }
  }
}
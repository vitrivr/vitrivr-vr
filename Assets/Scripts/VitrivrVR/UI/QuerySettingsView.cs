using System.Collections.Generic;
using UnityEngine;
using VitrivrVR.Query;
using VitrivrVR.Query.Display;

namespace VitrivrVR.UI
{
  public class QuerySettingsView : MonoBehaviour
  {
    public UITableController statisticsTable;
    public List<QueryDisplay> queryDisplayPrefabs;

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

    public void SetQueryDisplay(int displayPrefabIndex)
    {
      QueryController.Instance.queryDisplay = queryDisplayPrefabs[displayPrefabIndex];
    }

    public void SetObjectQueryDisplayMode()
    {
      // SetQueryDisplayMode(QueryDisplay.DisplayMode.MediaObjectDisplay);
    }

    public void SetSegmentQueryDisplayMode()
    {
      // SetQueryDisplayMode(QueryDisplay.DisplayMode.MediaSegmentDisplay);
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
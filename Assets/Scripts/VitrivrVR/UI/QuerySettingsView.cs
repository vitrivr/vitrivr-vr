using UnityEngine;
using VitrivrVR.Query;

namespace VitrivrVR.UI
{
  public class QuerySettingsView : MonoBehaviour
  {
    public UITableController statisticsTable;

    private void Awake()
    {
      QueryController.Instance.queryFocusEvent.AddListener(OnQueryFocus);

      statisticsTable.table = new[,]
      {
        {"Results", "-----"}
      };
    }

    private void OnQueryFocus(int oldIndex, int newIndex)
    {
      if (newIndex != -1)
      {
        var nResults = QueryController.Instance.queries[newIndex].display.NumberOfResults;
        statisticsTable.SetCell(0, 1, nResults.ToString());
      }
      else
      {
        statisticsTable.SetCell(0, 1, "-----");
      }
    }
  }
}
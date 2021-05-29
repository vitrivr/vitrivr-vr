using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Data;

namespace VitrivrVR.UI
{
  public class ConfigInfoView : MonoBehaviour
  {
    public GameObject scrollableUITable;

    private void Start()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var uiTable = Instantiate(scrollableUITable, transform);
      var uiTableController = uiTable.GetComponentInChildren<UITableController>();
      uiTableController.table = new[,]
      {
        {"Cineast Host", CineastConfigManager.Instance.Config.cineastHost},
        {"Media Host", CineastConfigManager.Instance.Config.mediaHost},
        {"Thumbnail Path", CineastConfigManager.Instance.Config.thumbnailPath},
        {"Thumbnail Extension", CineastConfigManager.Instance.Config.thumbnailExtension},
        {"Media Path", CineastConfigManager.Instance.Config.mediaPath}
      };
    }
  }
}
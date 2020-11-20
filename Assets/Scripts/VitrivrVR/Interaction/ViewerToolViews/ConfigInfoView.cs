using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using VitrivrVR.Data;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class ConfigInfoView : ViewerToolView
  {
    public GameObject scrollableUITable;

    private void Start()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var uiTable = Instantiate(scrollableUITable, transform);
      var uiTableController = uiTable.GetComponentInChildren<UITableController>();
      uiTableController.table = new[,]
      {
        {"Cineast Host", "Media Host", "Thumbnail Path", "Thumbnail Extension", "Media Path"},
        {
          CineastConfigManager.Instance.Config.cineastHost, CineastConfigManager.Instance.Config.mediaHost,
          CineastConfigManager.Instance.Config.thumbnailPath, CineastConfigManager.Instance.Config.thumbnailExtension,
          CineastConfigManager.Instance.Config.mediaPath
        }
      };
    }
  }
}
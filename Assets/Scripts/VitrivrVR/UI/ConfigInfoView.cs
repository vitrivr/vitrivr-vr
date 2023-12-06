using Dres.Unityclient;
using UnityEngine;
using VitrivrVR.Config;
using VitrivrVR.Query;

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
      var dresEnabled = ConfigManager.Config.dresEnabled;
      var cineastConfig = QueryController.Instance.GetCineastConfig();
      const string disabledMessage = "Disabled";
      uiTableController.table = new[,]
      {
        { "Cineast", "Host", cineastConfig.cineastHost },
        { "", "Cineast Serves Media", cineastConfig.cineastServesMedia.ToString() },
        { "", "Media Host", cineastConfig.mediaHost },
        { "", "Thumbnail Path", cineastConfig.thumbnailPath },
        { "", "Thumbnail Extension", cineastConfig.thumbnailExtension },
        { "", "Media Path", cineastConfig.mediaPath },
        { "", "", "" },

        { "Dres", "Host", dresEnabled ? DresConfigManager.Instance.Config.host : disabledMessage },
        { "", "Port", dresEnabled ? DresConfigManager.Instance.Config.port.ToString() : disabledMessage },
        { "", "tls", dresEnabled ? DresConfigManager.Instance.Config.tls.ToString() : disabledMessage },
        { "", "User", dresEnabled ? DresConfigManager.Instance.Config.user : disabledMessage },
        { "", "", "" },

        { "vitrivr-VR", "Max Results", ConfigManager.Config.maxResults.ToString() },
        { "", "Max Display", ConfigManager.Config.maxDisplay.ToString() },
        { "", "Dres enabled", ConfigManager.Config.dresEnabled.ToString() },
        { "", "Submission Replacement Regex", ConfigManager.Config.submissionIdReplacementRegex },
        { "", "Default Volume", ConfigManager.Config.defaultMediaVolume.ToString("F") },
        { "", "Skip Length", ConfigManager.Config.skipLength.ToString("F") }
      };
    }
  }
}
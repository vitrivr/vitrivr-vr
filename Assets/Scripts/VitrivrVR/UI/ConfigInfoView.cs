using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Utils;
using Vitrivr.UnityInterface.DresApi;
using VitrivrVR.Config;

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
      const string disabledMessage = "Disabled";
      uiTableController.table = new[,]
      {
        {"Cineast", "Host", CineastConfigManager.Instance.Config.cineastHost},
        {"", "Media Host", CineastConfigManager.Instance.Config.mediaHost},
        {"", "Thumbnail Path", CineastConfigManager.Instance.Config.thumbnailPath},
        {"", "Thumbnail Extension", CineastConfigManager.Instance.Config.thumbnailExtension},
        {"", "Media Path", CineastConfigManager.Instance.Config.mediaPath},
        {"", "", ""},
        
        {"Dres", "Host", dresEnabled ? DresConfigManager.Instance.Config.host : disabledMessage},
        {"", "Port", dresEnabled ? DresConfigManager.Instance.Config.port.ToString() : disabledMessage},
        {"", "tls", dresEnabled ? DresConfigManager.Instance.Config.tls.ToString() : disabledMessage},
        {"", "User", dresEnabled ? DresConfigManager.Instance.Config.user : disabledMessage},
        {"", "", ""},
        
        {"vitrivr-VR", "Max Results", ConfigManager.Config.maxResults.ToString()},
        {"", "Max Prefetch", ConfigManager.Config.maxPrefetch.ToString()},
        {"", "Max Display", ConfigManager.Config.maxDisplay.ToString()},
        {"", "Dres enabled", ConfigManager.Config.dresEnabled.ToString()},
        {"", "Submission Prefix Length", ConfigManager.Config.submissionIdPrefixLength.ToString()},
        {"", "Default Volume", ConfigManager.Config.defaultMediaVolume.ToString("F")},
        {"", "Skip Length", ConfigManager.Config.skipLength.ToString("F")}
      };
    }
  }
}
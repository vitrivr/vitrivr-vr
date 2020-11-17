using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using TMPro;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class ConfigInfoView : ViewerToolView
  {
    private void Start()
    {
      var text = GetComponent<TextMeshPro>();
      text.text = CineastConfigManager.Instance.Config.cineastHost;
    }
  }
}
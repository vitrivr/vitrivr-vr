using UnityEngine;
using Vitrivr.UnityInterface.DresApi;
using VitrivrVR.Config;

namespace VitrivrVR.Submission
{
  public class DresClientManager : MonoBehaviour
  {
    public static DresClient instance;

    private async void Start()
    {
      if (ConfigManager.Config.dresEnabled)
      {
        instance = new DresClient();
        await instance.Login();
      }
    }
  }
}
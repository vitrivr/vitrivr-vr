using UnityEngine;
using Vitrivr.UnityInterface.DresApi;
using VitrivrVR.Config;
using VitrivrVR.Notification;

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
        NotificationController.Notify($"Dres connected: {instance.UserDetails.Username}");
      }
    }
  }
}
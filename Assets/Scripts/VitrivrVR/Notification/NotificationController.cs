using System;
using UnityEngine;
using UnityEngine.Events;

namespace VitrivrVR.Notification
{
  public class NotificationController : MonoBehaviour
  {
    [Serializable]
    public class NotificationEvent : UnityEvent<string>
    {
    }

    private static NotificationController _instance;

    public NotificationEvent notificationEvent;

    private void Awake()
    {
      if (_instance != null)
      {
        Debug.LogError("NotificationController instance already registered! There are several " +
                       "NotificationControllers in the scene!");
      }
      else
      {
        _instance = this;
      }
    }

    public static void Notify(string notification)
    {
      if (_instance)
      {
        _instance.notificationEvent.Invoke(notification);
      }
      else
      {
        Debug.Log(notification);
      }
    }
  }
}
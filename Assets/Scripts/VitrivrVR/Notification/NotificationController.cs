using System;
using JetBrains.Annotations;
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

    public static void NotifyError(string error, [CanBeNull] Exception exception = null)
    {
      if (_instance)
      {
        _instance.notificationEvent.Invoke(error);
      }
      else
      {
        Debug.LogError(error);
      }

      if (exception == null) return;
      Debug.LogError(exception.StackTrace);
    }
  }
}
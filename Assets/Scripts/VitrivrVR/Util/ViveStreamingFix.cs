using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using VitrivrVR.Config;

namespace VitrivrVR.Util
{
  public class ViveStreamingFix : MonoBehaviour
  {
    public InputSystemUIInputModule uiInputModule;
    private float _timer;

    private void Awake()
    {
      if (ConfigManager.Config.viveStreamingFixEnabled) return;

      Destroy(this);
    }

    private void FixedUpdate()
    {
      _timer += Time.fixedDeltaTime;
      if (!(_timer >= 1)) return;

      var res0 = uiInputModule.GetLastRaycastResult(0);
      var res1 = uiInputModule.GetLastRaycastResult(1);
      if (!res0.isValid && !res1.isValid)
      {
        uiInputModule.enabled = false;
        uiInputModule.enabled = true;
      }

      _timer %= 1;
    }
  }
}
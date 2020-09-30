using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VitrivrVR.Input.Controller
{
  // Script structure from official documentation: https://docs.unity3d.com/Manual/xr_input.html

  public class XRButtonObserver : MonoBehaviour
  {
    [Serializable]
    public class XRButtonEvent : UnityEvent<bool> { }
    
    public XRButtonEvent primaryButtonEvent;

    private bool _lastPrimaryButtonState;
    private readonly List<InputDevice> _devicesWithPrimaryButton = new List<InputDevice>();

    private void OnEnable()
    {
      var allDevices = new List<InputDevice>();
      InputDevices.GetDevices(allDevices);
      foreach (var device in allDevices)
        DeviceConnected(device);

      InputDevices.deviceConnected += DeviceConnected;
      InputDevices.deviceDisconnected += DeviceDisconnected;
    }

    private void OnDisable()
    {
      InputDevices.deviceConnected -= DeviceConnected;
      InputDevices.deviceDisconnected -= DeviceDisconnected;
      _devicesWithPrimaryButton.Clear();
    }

    private void DeviceConnected(InputDevice device)
    {
      if (device.TryGetFeatureValue(CommonUsages.primaryButton, out _))
      {
        _devicesWithPrimaryButton.Add(device);
      }
    }

    private void DeviceDisconnected(InputDevice device)
    {
      if (_devicesWithPrimaryButton.Contains(device))
        _devicesWithPrimaryButton.Remove(device);
    }

    private void Update()
    {
      var primaryButtonState = false;
      foreach (var device in _devicesWithPrimaryButton)
      {
        bool tempState;
        primaryButtonState = device.TryGetFeatureValue(CommonUsages.primaryButton, out tempState)
          && tempState || primaryButtonState;
      }

      if (primaryButtonState == _lastPrimaryButtonState) return;
      primaryButtonEvent.Invoke(primaryButtonState);
      _lastPrimaryButtonState = primaryButtonState;
    }
  }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VitrivrVR.Input.Controller
{
  // Script structure from official documentation: https://docs.unity3d.com/Manual/xr_input.html

  /// <summary>
  /// Object for the management of XR controller button events.
  /// <para>Place in a scene and connect actions to the respective button events.</para>
  /// </summary>
  public class XRButtonObserver : MonoBehaviour
  {
    [Serializable]
    public class XRButtonEvent : UnityEvent<bool>
    {
    }

    public XRButtonEvent primaryButtonEvent;

    private bool _lastPrimaryButtonState;
    private readonly List<InputDevice> _devicesWithPrimaryButton = new List<InputDevice>();

    private void OnEnable()
    {
      // Get all already connected XR input devices
      var allDevices = new List<InputDevice>();
      InputDevices.GetDevices(allDevices);
      foreach (var device in allDevices)
        DeviceConnected(device);

      // Register this observer to be notified when new XR input devices are connected or existing ones disconnected
      InputDevices.deviceConnected += DeviceConnected;
      InputDevices.deviceDisconnected += DeviceDisconnected;
    }

    private void OnDisable()
    {
      // Deregister this observer from XR input device changes
      InputDevices.deviceConnected -= DeviceConnected;
      InputDevices.deviceDisconnected -= DeviceDisconnected;
      // Clear all device lists
      _devicesWithPrimaryButton.Clear();
    }

    /// <summary>
    /// Adds a newly connected XR input device to all relevant device lists.
    /// </summary>
    /// <param name="device">New XR input device to add to lists</param>
    private void DeviceConnected(InputDevice device)
    {
      if (device.TryGetFeatureValue(CommonUsages.primaryButton, out _))
      {
        _devicesWithPrimaryButton.Add(device);
      }
    }

    /// <summary>
    /// Removes a disconnecting XR input device from all device lists.
    /// </summary>
    /// <param name="device">XR input device to remove from lists</param>
    private void DeviceDisconnected(InputDevice device)
    {
      if (_devicesWithPrimaryButton.Contains(device))
        _devicesWithPrimaryButton.Remove(device);
    }

    private void Update()
    {
      // Get button states
      var primaryButtonState = false;
      foreach (var device in _devicesWithPrimaryButton)
      {
        primaryButtonState = device.TryGetFeatureValue(CommonUsages.primaryButton, out var tempState) && tempState
                             || primaryButtonState;
      }

      // Invoke events for which state has changed
      if (primaryButtonState == _lastPrimaryButtonState) return;
      primaryButtonEvent.Invoke(primaryButtonState);
      _lastPrimaryButtonState = primaryButtonState;
    }
  }
}
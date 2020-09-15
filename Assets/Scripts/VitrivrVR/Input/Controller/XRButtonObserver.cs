using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VitrivrVR.Input.Controller
{
    // Script structure from official documentation: https://docs.unity3d.com/Manual/xr_input.html

    public class XRButtonEvent : UnityEvent<bool> {}
    public class XRButtonObserver : MonoBehaviour
    {
        public XRButtonEvent primaryButtonEvent = new XRButtonEvent();
    
        private bool _lastButtonState;
        private List<InputDevice> _devicesWithPrimaryButton = new List<InputDevice>();

        void OnEnable()
        {
            List<InputDevice> allDevices = new List<InputDevice>();
            InputDevices.GetDevices(allDevices);
            foreach(InputDevice device in allDevices)
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

        void Update()
        {
            bool tempState = false;
            foreach (var device in _devicesWithPrimaryButton)
            {
                bool primaryButtonState;
                tempState = device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonState)
                    && primaryButtonState || tempState;
            }

            if (tempState != _lastButtonState)
            {
                primaryButtonEvent.Invoke(tempState);
                _lastButtonState = tempState;
            }
        }
    }
}
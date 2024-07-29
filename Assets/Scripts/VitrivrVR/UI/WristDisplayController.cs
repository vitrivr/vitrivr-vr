using System;
using UnityEngine;

namespace VitrivrVR.UI
{
  public class WristDisplayController : MonoBehaviour
  {
    private GameObject _display;
    private Transform _camera;

    private void Start()
    {
      // Get child
      _display = transform.GetChild(0).gameObject;
      // Get main camera
      if (Camera.main != null) _camera = Camera.main.transform;
    }

    private void Update()
    {
      // In the update we check if the camera is looking at the wrist mounted display.
      // If it is, we enable the display, otherwise we disable it.
      _display.SetActive(_camera != null && Vector3.Dot(_camera.forward, transform.position - _camera.position) > .5f);
    }
  }
}
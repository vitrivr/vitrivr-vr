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
      // In the update we check if the wrist mounted display is angled towards the camera.
      // If it is, we enable the display, otherwise we disable it.
      if (!_camera) return;
      var toWristDisplay = _camera.position - transform.position;
      var dotProduct = Vector3.Dot(-_display.transform.forward, toWristDisplay);
      var magnitudeProduct = _camera.forward.magnitude * toWristDisplay.magnitude;
      var cosineSimilarity = dotProduct / magnitudeProduct;

      _display.SetActive(cosineSimilarity > .75f);
    }
  }
}
using UnityEngine;

namespace VitrivrVR.Behavior
{
  public class PulseController : MonoBehaviour
  {
    public float pulseCycleTime = 1;
    public float pulseStrength = 0.25f;

    private float _pulseTime;

    private void Update()
    {
      _pulseTime += Time.deltaTime;
      _pulseTime %= pulseCycleTime;

      var sizeDelta = Mathf.Sin(2 * Mathf.PI * (_pulseTime / pulseCycleTime)) * pulseStrength;
      var size = 1 + sizeDelta;
      
      transform.localScale = new Vector3(size, size, size);
    }
  }
}
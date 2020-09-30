using UnityEngine;

public class SpinnerController : MonoBehaviour
{
  public float spinSpeed = 180;

  void Update()
  {
    transform.Rotate(Vector3.back, Time.deltaTime * spinSpeed);
  }
}
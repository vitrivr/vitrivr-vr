using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    public float spinSpeed = 180;
    void Update()
    {
        transform.Rotate(Vector3.back, Time.deltaTime * spinSpeed);
    }
}

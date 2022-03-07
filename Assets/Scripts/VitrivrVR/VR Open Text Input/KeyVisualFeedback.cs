using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyVisualFeedback : MonoBehaviour
{
    private KeyAudioFeedback soundHandler;
    public bool keyHit = false;
    public bool keyReset = false;

    private float originalY;
    // Start is called before the first frame update
    void Start()
    {
        soundHandler = GameObject.FindGameObjectWithTag("SoundHandler").GetComponent<KeyAudioFeedback>();
        originalY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (keyHit)
        {
            originalY = transform.position.y;
            soundHandler.PlayKeyClick();
            keyReset = false;
            keyHit = false;
            transform.position += new Vector3(0, -0.01f, 0);
        }

        if (transform.position.y < originalY)
        {
            transform.position += new Vector3(0, 0.002f, 0);
        }
        else
        {
            keyReset = true;
        }
    }
}

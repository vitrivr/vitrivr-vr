using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPreviewController : MonoBehaviour
{
    public Texture2D errorTexture;
    public string URL { get; set; }
    
    private VideoPlayer _videoPlayer;
    void Start()
    {
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        var audioSource = gameObject.AddComponent<AudioSource>();
        
        _videoPlayer.url = URL;

        _videoPlayer.isLooping = true;
        _videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        _videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
        _videoPlayer.targetMaterialProperty = "_MainTex";
        
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _videoPlayer.SetTargetAudioSource(0, audioSource);
        _videoPlayer.prepareCompleted += PrepareCompleted;
        _videoPlayer.errorReceived += ErrorEncountered;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (_videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
            }
            else
            {
                _videoPlayer.Play();
            }
        }
    }

    void PrepareCompleted(VideoPlayer videoPlayer)
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        float factor = Mathf.Max(videoPlayer.width, videoPlayer.height);
        Vector3 scale = new Vector3(videoPlayer.width / factor, videoPlayer.height / factor, 1);
        transform.localScale = scale;
    }

    void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
        Debug.LogError(error);
        GetComponent<Renderer>().material.mainTexture = errorTexture;
    }
}

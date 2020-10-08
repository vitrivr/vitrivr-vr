using UnityEngine;
using UnityEngine.Video;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Legacy media display exclusively for videos.
  /// </summary>
  public class VideoPreviewController : MonoBehaviour
  {
    public Texture2D errorTexture;
    public string URL { get; set; }

    private VideoPlayer _videoPlayer;

    private void Start()
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

    private void Update()
    {
      if (!UnityEngine.Input.GetButtonDown("Jump")) return;
      if (_videoPlayer.isPlaying)
      {
        _videoPlayer.Pause();
      }
      else
      {
        _videoPlayer.Play();
      }
    }

    private void PrepareCompleted(VideoPlayer videoPlayer)
    {
      if (videoPlayer.isPlaying)
      {
        videoPlayer.Pause();
      }

      var factor = Mathf.Max(videoPlayer.width, videoPlayer.height);
      var scale = new Vector3(videoPlayer.width / factor, videoPlayer.height / factor, 1);
      transform.localScale = scale;
    }

    private void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      GetComponent<Renderer>().material.mainTexture = errorTexture;
    }
  }
}
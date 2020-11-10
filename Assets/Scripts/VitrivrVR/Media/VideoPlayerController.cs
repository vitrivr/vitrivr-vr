using System;
using UnityEngine;
using UnityEngine.Video;

namespace VitrivrVR.Media
{
  public class VideoPlayerController
  {
    private VideoPlayer _videoPlayer;
    private Action<RenderTexture> _prepareComplete;

    public VideoPlayerController(GameObject gameObject, string mediaUrl, long startFrame,
      Action<RenderTexture> prepareComplete, VideoPlayer.ErrorEventHandler errorHandler)
    {
      _videoPlayer = gameObject.AddComponent<VideoPlayer>();
      var audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.spatialize = true;
      audioSource.spatialBlend = 1;

      _videoPlayer.isLooping = true;
      _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

      _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      _videoPlayer.SetTargetAudioSource(0, audioSource);
      _videoPlayer.prepareCompleted += PrepareCompleted;
      _prepareComplete = prepareComplete;
      _videoPlayer.errorReceived += errorHandler;

      _videoPlayer.playOnAwake = true;

      _videoPlayer.url = mediaUrl;
      _videoPlayer.frame = startFrame;
    }

    public int Width => (int)_videoPlayer.width;
    public int Height => (int)_videoPlayer.height;
    public double Length => _videoPlayer.length;
    public bool IsPlaying => _videoPlayer.isPlaying;
    public double Time => _videoPlayer.time;

    public void Pause()
    {
      _videoPlayer.Pause();
    }

    public void Play()
    {
      _videoPlayer.Play();
    }

    private void PrepareCompleted(VideoPlayer videoPlayer)
    {
      var renderTex = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 24);
      videoPlayer.targetTexture = renderTex;

      videoPlayer.Pause();

      _prepareComplete(renderTex);
    }
  }
}
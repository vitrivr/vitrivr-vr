using System;
using UnityEngine;
using UnityEngine.Video;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Wrapper class for the <see cref="VideoPlayer"/> class, combining all required logic into a single class.
  /// Renders the loaded video to a <see cref="RenderTexture"/> returned on prepareComplete.
  /// </summary>
  public class VideoPlayerController
  {
    private readonly VideoPlayer _videoPlayer;
    private readonly Action<RenderTexture> _prepareComplete;
    private readonly AudioSource _audioSource;

    public VideoPlayerController(GameObject gameObject, string mediaUrl, long startFrame,
      Action<RenderTexture> prepareComplete, VideoPlayer.ErrorEventHandler errorHandler)
    {
      _videoPlayer = gameObject.AddComponent<VideoPlayer>();
      _audioSource = gameObject.AddComponent<AudioSource>();
      _audioSource.spatialize = true;
      _audioSource.spatialBlend = 1;

      _videoPlayer.isLooping = true;
      _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

      _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      _videoPlayer.SetTargetAudioSource(0, _audioSource);
      _videoPlayer.prepareCompleted += PrepareCompleted;
      _prepareComplete = prepareComplete;
      _videoPlayer.errorReceived += errorHandler;

      _videoPlayer.playOnAwake = true;

      _videoPlayer.url = mediaUrl;
      _videoPlayer.frame = startFrame;
    }

    public int Width => (int) _videoPlayer.width;
    public int Height => (int) _videoPlayer.height;
    public double Length => _videoPlayer.length;
    public long FrameCount => (long) _videoPlayer.frameCount;
    public bool IsPlaying => _videoPlayer.isPlaying;
    public double Time => _videoPlayer.time;
    public double ClockTime => _videoPlayer.clockTime;

    public Texture2D GetCurrentFrame()
    {
      // Store active render texture
      var activeTexture = RenderTexture.active;

      var texture = _videoPlayer.targetTexture;

      // Set video texture active
      RenderTexture.active = texture;

      var frame = new Texture2D(texture.width, texture.height);
      frame.ReadPixels(new Rect(0, 0, frame.width, frame.height), 0, 0);
      frame.Apply();

      // Set active render texture back
      RenderTexture.active = activeTexture;
      return frame;
    }

    public void Pause()
    {
      _videoPlayer.Pause();
    }

    public void Play()
    {
      _videoPlayer.Play();
    }

    public void SetFrame(long frame)
    {
      _videoPlayer.frame = frame;
    }

    public void SetTime(double time)
    {
      _videoPlayer.time = time;
    }

    public void SetVolume(float volume)
    {
      _audioSource.volume = volume;
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
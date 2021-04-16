using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace VitrivrVR.Media.Controller
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

      _videoPlayer.playOnAwake = false;
      _videoPlayer.waitForFirstFrame = true;

      _videoPlayer.url = mediaUrl;
      _videoPlayer.frame = startFrame;
      
      _videoPlayer.Prepare();
    }

    public int Width => (int) _videoPlayer.width;
    public int Height => (int) _videoPlayer.height;
    public double Length => _videoPlayer.length;
    public long FrameCount => (long) _videoPlayer.frameCount;
    public bool IsPlaying => _videoPlayer.isPlaying;
    public double Time => _videoPlayer.time;
    public double ClockTime => _videoPlayer.clockTime;
    public long Frame => _videoPlayer.frame;

    public Texture2D GetCurrentFrame()
    {
      var texture = _videoPlayer.targetTexture;
      var frame = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

      // TODO: Figure out how to use CopyTexture for efficient texture copying
      // if (SystemInfo.copyTextureSupport == CopyTextureSupport.None)
      // {
      // Store active render texture
      var activeTexture = RenderTexture.active;

      // Set video texture active
      RenderTexture.active = texture;

      frame.ReadPixels(new Rect(0, 0, frame.width, frame.height), 0, 0);
      frame.Apply();

      // Set active render texture back
      RenderTexture.active = activeTexture;
      // }
      // else
      // {
      //   Graphics.CopyTexture(texture, frame);
      // }

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

    private async void PrepareCompleted(VideoPlayer videoPlayer)
    {
      for (var i = 0; i < 500 && (videoPlayer.width == 0 || videoPlayer.height == 0); i++)
      {
        // Video player did not fully prepare yet, waiting for delay
        await Task.Delay(1);
      }

      if (videoPlayer.width == 0 || videoPlayer.height == 0)
      {
        Debug.LogError($"Could not correctly load video: {videoPlayer.url}");
        return;
      }

      var renderTex = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 0);
      videoPlayer.targetTexture = renderTex;

      videoPlayer.Pause();

      _prepareComplete(renderTex);
    }
  }
}
using System;
using System.Collections;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using VitrivrVR.Config;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Canvas based <see cref="MediaItemDisplay"/>.
  /// </summary>
  public class CanvasMediaItemDisplay : MediaItemDisplay
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;
    public RawImage scoreFrame;
    public RectTransform imageFrame;
    public TextMeshProUGUI segmentDataText;
    public int scoreFrameSize = 25;
    public RectTransform progressBar;
    public RectTransform progressIndicator;
    public RectTransform segmentIndicator;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private bool _videoInitialized;
    private VideoPlayer _videoPlayer;

    /// <summary>
    /// Tiny class for the sole purpose of enabling click events on <see cref="CanvasMediaItemDisplay"/> instances.
    /// </summary>
    private class ClickHandler : MonoBehaviour, IPointerClickHandler
    {
      public Action onClick;

      public void OnPointerClick(PointerEventData eventData)
      {
        onClick();
      }
    }

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClick;
    }

    private void Start()
    {
      var progressBarSizeDelta = progressBar.sizeDelta;
      progressBarSizeDelta.y = scoreFrameSize;
      progressBar.sizeDelta = progressBarSizeDelta;
      var progressBarPos = progressBar.anchoredPosition;
      progressBarPos.y = -scoreFrameSize / 2f;
      progressBar.anchoredPosition = progressBarPos;
    }

    private void Update()
    {
      if (_videoInitialized && _videoPlayer.isPlaying)
      {
        UpdateProgressIndicator(_videoPlayer.time);
      }
    }

    private void UpdateProgressIndicator(double time)
    {
      progressIndicator.anchoredPosition =
        new Vector2((float) (progressBar.rect.width * time / _videoPlayer.length), 0);
    }

    private void SetSegmentIndicator(double start, double end, double length)
    {
      var rect = progressBar.rect;
      segmentIndicator.anchoredPosition = new Vector2((float) (rect.width * start / length), 0);
      segmentIndicator.sizeDelta = new Vector2((float) (rect.width * (end - start) / length), 0);
    }

    /// <summary>
    /// Initializes this display with the given segment data.
    /// </summary>
    /// <param name="segment">Segment to display</param>
    public override async Task Initialize(ScoredSegment segment)
    {
      _scoredSegment = segment;
      _segment = segment.segment;
      var config = CineastConfigManager.Instance.Config;
      var vrConfig = ConfigManager.Config;
      var score = (float) _scoredSegment.score;
      segmentDataText.text = $"Segment {_segment.Id}\nScore: {score:F}";
      // Score frame
      scoreFrame.color = vrConfig.similarityColor.ToColor() * score +
                         vrConfig.dissimilarityColor.ToColor() * (1 - score);
      var rectTransform = scoreFrame.rectTransform;
      rectTransform.offsetMin = new Vector2(-scoreFrameSize, 0);
      rectTransform.offsetMax = new Vector2(scoreFrameSize, scoreFrameSize);
      scoreFrame.gameObject.SetActive(true);
      try
      {
        var objectId = await _segment.GetObjectId();
        var thumbnailPath = PathResolver.ResolvePath(config.thumbnailPath, objectId, _segment.Id);
        var thumbnailUrl = $"{config.mediaHost}{thumbnailPath}{config.thumbnailExtension}";
        StartCoroutine(DownloadThumbnailTexture(thumbnailUrl));
      }
      catch (Exception)
      {
        previewImage.texture = errorTexture;
      }
    }

    private void OnClick()
    {
      if (_videoInitialized)
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
      else
      {
        InitializeVideo();
      }
    }

    /// <summary>
    /// Initializes the <see cref="VideoPlayer"/> component of this display.
    /// </summary>
    private async void InitializeVideo()
    {
      // Check if segment has encountered error during initialization
      if (!_segment.Initialized)
      {
        // Try again to initialize
        try
        {
          await _segment.GetObjectId();
        }
        catch (Exception)
        {
          return;
        }
      }

      // Set flag here to ensure video is only initialized once
      _videoInitialized = true;

      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      imageFrame.sizeDelta = new Vector2(1000, 1000);

      // Resolve media URL
      // TODO: Retrieve and / or apply all required media information, potentially from within PathResolver
      var config = CineastConfigManager.Instance.Config;
      var objectId = await _segment.GetObjectId();
      var mediaPath = PathResolver.ResolvePath(config.mediaPath, objectId);
      var mediaUrl = $"{config.mediaHost}{mediaPath}";

      _videoPlayer = gameObject.AddComponent<VideoPlayer>();
      var audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.spatialize = true;
      audioSource.spatialBlend = 1;

      _videoPlayer.isLooping = true;
      _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

      _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      _videoPlayer.SetTargetAudioSource(0, audioSource);
      _videoPlayer.prepareCompleted += PrepareCompleted;
      _videoPlayer.errorReceived += ErrorEncountered;

      _videoPlayer.playOnAwake = true;

      _videoPlayer.url = mediaUrl;
      _videoPlayer.frame = await _segment.GetStart();

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      segmentDataText.text =
        $"Segment {_segment.Id} (Object {objectId}): {start:F}s - {end:F}s\nScore: {_scoredSegment.score:F}";
    }

    /// <summary>
    /// Method to download and apply the thumbnail texture from the given URL. Start as <see cref="Coroutine"/>.
    /// </summary>
    /// <param name="url">The URL to the thumbnail file</param>
    private IEnumerator DownloadThumbnailTexture(string url)
    {
      var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.isNetworkError || www.isHttpError)
      {
        Debug.LogError(www.error);
        previewImage.texture = errorTexture;
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        previewImage.texture = loadedTexture;
        float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
        imageFrame.sizeDelta =
          new Vector2(1000 * loadedTexture.width / factor, 1000 * loadedTexture.height / factor);
      }
    }

    private void PrepareCompleted(VideoPlayer videoPlayer)
    {
      var factor = Mathf.Max(videoPlayer.width, videoPlayer.height);
      var renderTex = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 24);
      videoPlayer.targetTexture = renderTex;
      previewImage.texture = renderTex;
      imageFrame.sizeDelta =
        new Vector2(1000 * videoPlayer.width / factor, 1000 * videoPlayer.height / factor);
      
      segmentDataText.rectTransform.anchoredPosition -= new Vector2(0, scoreFrameSize);

      var start = _segment.GetAbsoluteStart().Result;
      var end = _segment.GetAbsoluteEnd().Result;
      var length = _videoPlayer.length;
      UpdateProgressIndicator(start);
      SetSegmentIndicator(start, end, length);
      progressBar.gameObject.SetActive(true);

      videoPlayer.Pause();
    }

    private void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      previewImage.texture = errorTexture;
    }
  }
}
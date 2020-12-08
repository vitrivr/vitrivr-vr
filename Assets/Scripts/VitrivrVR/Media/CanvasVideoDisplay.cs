using System;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using VitrivrVR.Util;

namespace VitrivrVR.Media
{
  public class CanvasVideoDisplay : MonoBehaviour
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;
    public RectTransform progressBar;
    public RectTransform progressIndicator;
    public RectTransform segmentIndicator;
    public TextMeshProUGUI segmentDataText;
    public float progressBarSize = 100;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private VideoPlayerController _videoPlayerController;
    private RectTransform _imageTransform;

    public async void Initialize(ScoredSegment segment)
    {
      _scoredSegment = segment;
      _segment = _scoredSegment.segment;
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

      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      _imageTransform.sizeDelta = new Vector2(1000, 1000);

      // Resolve media URL
      // TODO: Retrieve and / or apply all required media information, potentially from within PathResolver
      var config = CineastConfigManager.Instance.Config;
      var objectId = await _segment.GetObjectId();
      var mediaPath = PathResolver.ResolvePath(config.mediaPath, objectId);
      var mediaUrl = $"{config.mediaHost}{mediaPath}";

      var startFrame = await _segment.GetStart();

      _videoPlayerController =
        new VideoPlayerController(gameObject, mediaUrl, startFrame, PrepareCompleted, ErrorEncountered);

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      segmentDataText.text = $"Segment {_segment.Id}: {start:F}s - {end:F}s\nScore: {_scoredSegment.score:F}";

      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClickImage;
      var progressClickHandler = progressBar.gameObject.AddComponent<ClickHandler>();
      progressClickHandler.onClick = OnClickProgressBar;
    }

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      _imageTransform = previewImage.GetComponent<RectTransform>();
    }
    
    private void Update()
    {
      if (_videoPlayerController != null && _videoPlayerController.IsPlaying)
      {
        UpdateProgressIndicator(_videoPlayerController.ClockTime);
      }
    }

    private void OnClickImage(PointerEventData pointerEventData)
    {
      if (_videoPlayerController.IsPlaying)
      {
        _videoPlayerController.Pause();
      }
      else
      {
        _videoPlayerController.Play();
      }
    }
    
    private void OnClickProgressBar(PointerEventData pointerEventData)
    {
      var clickPosition = Quaternion.Inverse(Quaternion.LookRotation(progressBar.forward)) *
                          (pointerEventData.pointerCurrentRaycast.worldPosition - progressBar.position);
      var corners = new Vector3[4];
      progressBar.GetWorldCorners(corners);
      var progressBarWidth = (corners[1] - corners[2]).magnitude;

      var clickProgress = clickPosition.x / progressBarWidth + 0.5;
      var newTime = _videoPlayerController.Length * clickProgress;
      if (_videoPlayerController.IsPlaying)
      {
        _videoPlayerController.Pause();
        _videoPlayerController.SetTime(newTime);
        _videoPlayerController.Play();
      }
      else
      {
        _videoPlayerController.SetTime(newTime);
      }

      UpdateProgressIndicator(newTime);
    }

    private void PrepareCompleted(RenderTexture texture)
    {
      var width = _videoPlayerController.Width;
      var height = _videoPlayerController.Height;
      var factor = Mathf.Max(width, height);
      previewImage.texture = texture;
      _imageTransform.sizeDelta = new Vector2(1000f * width / factor, 1000f * height / factor);
      
      segmentDataText.rectTransform.anchoredPosition -= new Vector2(0, progressBarSize);

      var start = _segment.GetAbsoluteStart().Result;
      var end = _segment.GetAbsoluteEnd().Result;
      var length = _videoPlayerController.Length;
      UpdateProgressIndicator(start);
      SetSegmentIndicator(start, end, length);
      // Set progress bar size
      var progressBarSizeDelta = progressBar.sizeDelta;
      progressBarSizeDelta.y = progressBarSize;
      progressBar.sizeDelta = progressBarSizeDelta;
      var progressBarPos = progressBar.anchoredPosition;
      progressBarPos.y = -progressBarSize / 2f;
      progressBar.anchoredPosition = progressBarPos;
      progressBar.gameObject.SetActive(true);
    }

    private void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      previewImage.texture = errorTexture;
    }
    
    private void UpdateProgressIndicator(double time)
    {
      progressIndicator.anchoredPosition =
        new Vector2((float) (progressBar.rect.width * time / _videoPlayerController.Length), 0);
    }

    private void SetSegmentIndicator(double start, double end, double length)
    {
      var rect = progressBar.rect;
      segmentIndicator.anchoredPosition = new Vector2((float) (rect.width * start / length), 0);
      segmentIndicator.sizeDelta = new Vector2((float) (rect.width * (end - start) / length), 0);
    }
  }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Registries;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using VitrivrVR.Config;
using VitrivrVR.Query;
using VitrivrVR.Util;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Canvas based video player.
  /// </summary>
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

    public MediaObjectSegmentView mediaObjectSegmentViewPrefab;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private List<SegmentData> _segments;
    private VideoPlayerController _videoPlayerController;
    private RectTransform _imageTransform;
    private Action _onClose;
    private MediaObjectSegmentView _objectSegmentView;

    /// <summary>
    /// Number of segment indicators to instantiate each frame in Coroutine.
    /// </summary>
    private const int InstantiationBatch = 100;

    public async void Initialize(ScoredSegment segment, Action onClose)
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
      segmentDataText.text = $"Segment {_segment.Id}:\n{start:F}s - {end:F}s\nScore: {_scoredSegment.score:F}";

      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClickImage;
      var progressClickHandler = progressBar.gameObject.AddComponent<ClickHandler>();
      progressClickHandler.onClick = OnClickProgressBar;
      _onClose = onClose;
    }

    public void Close()
    {
      _onClose();
    }

    public async void ShowObjectSegmentView()
    {
      if (_objectSegmentView)
      {
        Destroy(_objectSegmentView.gameObject);
      }
      else
      {
        var t = transform;
        var mediaObject = ObjectRegistry.GetObject(await _segment.GetObjectId());
        _objectSegmentView = Instantiate(mediaObjectSegmentViewPrefab, t.position - 0.2f * t.forward, t.rotation, t);
        _objectSegmentView.Initialize(mediaObject);
      }
    }

    public void SetVolume(float volume)
    {
      _videoPlayerController.SetVolume(volume);
    }

    public void QueryByCurrentFrame()
    {
      var frame = _videoPlayerController.GetCurrentFrame();
      var term = QueryTermBuilder.BuildImageTermForCategories(frame, ConfigManager.Config.defaultImageCategories);
      QueryController.Instance.RunQuery(new List<QueryTerm> {term});
    }

    private void Awake()
    {
      GetComponentInChildren<Canvas>().worldCamera = Camera.main;
      _imageTransform = previewImage.GetComponent<RectTransform>();
    }

    private void Update()
    {
      if (_videoPlayerController != null && _videoPlayerController.IsPlaying)
      {
        var time = _videoPlayerController.ClockTime;
        UpdateProgressIndicator(time);
        UpdateText(time);
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
      UpdateText(newTime);
    }

    private async void PrepareCompleted(RenderTexture texture)
    {
      var width = _videoPlayerController.Width;
      var height = _videoPlayerController.Height;
      var factor = Mathf.Max(width, height);
      previewImage.texture = texture;
      _imageTransform.sizeDelta = new Vector2(1000f * width / factor, 1000f * height / factor);

      segmentDataText.rectTransform.anchoredPosition -= new Vector2(0, progressBarSize);

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      var length = _videoPlayerController.Length;
      UpdateProgressIndicator(start);
      SetSegmentIndicator(start, end, length, segmentIndicator);
      // Set progress bar size
      var progressBarSizeDelta = progressBar.sizeDelta;
      progressBarSizeDelta.y = progressBarSize;
      progressBar.sizeDelta = progressBarSizeDelta;
      var progressBarPos = progressBar.anchoredPosition;
      progressBarPos.y = -progressBarSize / 2f;
      progressBar.anchoredPosition = progressBarPos;
      progressBar.gameObject.SetActive(true);

      // Instantiate segment indicators
      var mediaObject = ObjectRegistry.GetObject(await _segment.GetObjectId());
      _segments = await mediaObject.GetSegments();
      var segmentStarts = (await Task.WhenAll(
          _segments.Where(segment => segment != _segment)
            .Select(segment => segment.GetAbsoluteStart())))
        .Where(segStart => segStart != 0);
      StartCoroutine(InstantiateSegmentIndicators(segmentStarts));
    }

    /// <summary>
    /// Coroutine to batch instantiate segment indicators.
    /// </summary>
    /// <param name="segmentStarts">Segment start times in seconds</param>
    /// <returns></returns>
    private IEnumerator InstantiateSegmentIndicators(IEnumerable<float> segmentStarts)
    {
      var i = 0;
      foreach (var segStart in segmentStarts)
      {
        var indicator = Instantiate(progressIndicator, segmentIndicator.parent);
        indicator.SetSiblingIndex(0);
        indicator.anchoredPosition =
          new Vector2((float) (progressBar.rect.width * segStart / _videoPlayerController.Length), 0);
        indicator.sizeDelta = new Vector2(1, 0);
        indicator.GetComponent<RawImage>().color = Color.black;
        i++;
        if (i == InstantiationBatch)
        {
          i = 0;
          yield return null;
        }
      }
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

    private async void UpdateText(double time)
    {
      if (_segments == null)
        return;
      var mediaObjectId = await _segment.GetObjectId();
      foreach (var segment in _segments)
      {
        var start = await segment.GetAbsoluteStart();
        var end = await segment.GetAbsoluteEnd();

        if (start <= time && time <= end)
        {
          var current = TimeSpan.FromSeconds(time).ToString("g");
          segmentDataText.text =
            $"{mediaObjectId}: {current}\nCurrent: {segment.Id}";
          break;
        }
      }
    }

    private void SetSegmentIndicator(double start, double end, double length, RectTransform rt)
    {
      var rect = progressBar.rect;
      rt.anchoredPosition = new Vector2((float) (rect.width * start / length), 0);
      rt.sizeDelta = new Vector2((float) (rect.width * (end - start) / length), 0);
    }
  }
}
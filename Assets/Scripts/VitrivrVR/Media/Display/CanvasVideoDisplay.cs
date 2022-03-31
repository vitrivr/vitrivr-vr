using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;
using VitrivrVR.Media.Controller;
using VitrivrVR.Notification;
using VitrivrVR.Query;
using VitrivrVR.Submission;
using VitrivrVR.UI;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Canvas based video player.
  /// </summary>
  public class CanvasVideoDisplay : MediaDisplay
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;
    public RectTransform progressBar;
    public RectTransform progressIndicator;
    public RectTransform segmentIndicator;
    public TextMeshProUGUI segmentDataText;
    public GameObject scrollableUITable;
    public GameObject submitButton;

    public GameObject mediaObjectSegmentViewPrefab;
    public ScrollRect scrollableListPrefab;
    public GameObject listItemPrefab;

    public Slider volumeSlider;

    public InputAction skipAction;
    public HoverHandler videoHoverHandler;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private ObjectData _mediaObject;
    private List<SegmentData> _segments;
    private VideoPlayerController _videoPlayerController;
    private RectTransform _imageTransform;
    private Action _onClose;
    private GameObject _objectSegmentView;
    private bool _metadataShown;
    private GameObject _metadataTable;
    private bool _tagListShown;
    private ScrollRect _tagList;

    private bool _hovered;

    /// <summary>
    /// Number of segment indicators to instantiate each frame in Coroutine.
    /// </summary>
    private const int InstantiationBatch = 100;

    public override async void Initialize(ScoredSegment segment, Action onClose)
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

      _mediaObject = ObjectRegistry.GetObject(await _segment.GetObjectId());

      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      _imageTransform.sizeDelta = new Vector2(1000, 1000);

      // Resolve media URL
      var mediaUrl = await CineastWrapper.GetMediaUrlOfAsync(_mediaObject, _segment.Id);

      var startFrame = await _segment.GetStart();

      _videoPlayerController =
        new VideoPlayerController(gameObject, mediaUrl, startFrame, PrepareCompleted, ErrorEncountered);

      var volume = ConfigManager.Config.defaultMediaVolume;
      SetVolume(volume);
      volumeSlider.value = volume;

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      segmentDataText.text = $"Segment {_segment.Id}:\n{start:F}s - {end:F}s\nScore: {_scoredSegment.score:F}";

      var progressClickHandler = progressBar.gameObject.AddComponent<ClickHandler>();
      progressClickHandler.onClick = OnClickProgressBar;
      _onClose = onClose;

      // Enable DRES submission button
      if (ConfigManager.Config.dresEnabled)
      {
        submitButton.SetActive(true);
        DresClientManager.LogInteraction("videoPlayer", $"initialized {_mediaObject.Id} {_segment.Id}");
      }
    }

    public void Close()
    {
      Destroy(gameObject);
      _onClose();
    }

    private void OnDestroy()
    {
      if (_objectSegmentView)
      {
        Destroy(_objectSegmentView);
      }

      DresClientManager.LogInteraction("videoPlayer", $"closed {_mediaObject.Id} {_segment.Id}");
    }

    public void ShowObjectSegmentView()
    {
      if (_objectSegmentView)
      {
        Destroy(_objectSegmentView.gameObject);
      }
      else
      {
        var t = transform;
        _objectSegmentView = Instantiate(mediaObjectSegmentViewPrefab, t.position - 0.2f * t.forward, t.rotation);
        _objectSegmentView.GetComponentInChildren<MediaObjectSegmentView>()
          .Initialize(_mediaObject, (i, _) => SkipToSegment(i));
      }
    }

    public async void ToggleMetadata()
    {
      if (_metadataShown)
      {
        Destroy(_metadataTable);
        _metadataShown = false;
        DresClientManager.LogInteraction("mediaObjectMetadata", $"closed {_mediaObject.Id}");
        return;
      }

      _metadataShown = true;

      var metadata = await _mediaObject.Metadata.GetAll();
      var rows = metadata.Values.Select(domain => domain.Count).Aggregate(0, (x, y) => x + y);
      var table = new string[rows, 3];
      var i = 0;
      foreach (var domain in metadata.Where(domain => domain.Value.Count != 0))
      {
        // Fill first column
        table[i, 0] = domain.Key;
        for (var j = 1; j < domain.Value.Count; j++)
        {
          table[i + j, 0] = "";
        }

        // Fill key-value pairs
        foreach (var (pair, index) in domain.Value.Select((pair, index) => (pair, index)))
        {
          table[i + index, 1] = pair.Key;
          table[i + index, 2] = pair.Value;
        }

        i += domain.Value.Count;
      }

      var bottomStack = progressBar.parent;

      _metadataTable = Instantiate(scrollableUITable, bottomStack);
      var uiTableController = _metadataTable.GetComponentInChildren<UITableController>();
      uiTableController.table = table;
      var uiTableTransform = _metadataTable.GetComponent<RectTransform>();
      uiTableTransform.sizeDelta = new Vector2(100, 600); // x is completely irrelevant here, since width is auto

      DresClientManager.LogInteraction("mediaObjectMetadata", $"opened {_mediaObject.Id}");
    }

    public async void ToggleTagList()
    {
      if (_tagListShown)
      {
        Destroy(_tagList.gameObject);
        _tagListShown = false;
        DresClientManager.LogInteraction("segmentTags", $"closed {_mediaObject.Id}");
        return;
      }

      _tagListShown = true;

      var bottomStack = progressBar.parent;

      _tagList = Instantiate(scrollableListPrefab, bottomStack);
      var listRect = _tagList.GetComponent<RectTransform>();
      listRect.anchorMin = new Vector2(0, .5f);
      listRect.anchorMax = new Vector2(0, .5f);
      listRect.sizeDelta = new Vector2(100, 600);

      var listContent = _tagList.content;

      // TODO: Preload or cache for all results
      var segment = await GetCurrentSegment(_videoPlayerController.ClockTime);
      var tagIds = await CineastWrapper.MetadataApi.FindTagInformationByIdAsync(segment.Id);

      var tags = await CineastWrapper.TagApi.FindTagsByIdAsync(new IdList(tagIds.TagIDs));

      foreach (var tagData in tags.Tags)
      {
        var tagItem = Instantiate(listItemPrefab, listContent);
        tagItem.GetComponentInChildren<TextMeshProUGUI>().text = tagData.Name;
      }

      DresClientManager.LogInteraction("segmentTags", $"opened {_mediaObject.Id} {segment.Id}");
    }

    public void SubmitCurrentFrame()
    {
      if (!ConfigManager.Config.dresEnabled)
      {
        NotificationController.Notify("Dres is disabled!");
        return;
      }

      var frame = _videoPlayerController.Frame;

      DresClientManager.SubmitResult(_mediaObject.Id, (int) frame);
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

      skipAction.started += Skip;
      videoHoverHandler.onEnter += OnHoverEnter;
      videoHoverHandler.onExit += OnHoverExit;
    }

    private void OnEnable()
    {
      skipAction.Enable();
    }

    private void OnDisable()
    {
      skipAction.Disable();
    }

    private void OnHoverEnter(PointerEventData pointerEventData)
    {
      _hovered = true;
    }

    private void OnHoverExit(PointerEventData pointerEventData)
    {
      _hovered = false;
    }

    /// <summary>
    /// Skips the set number of seconds forward or backward if this video is hovered.
    /// </summary>
    private void Skip(InputAction.CallbackContext context)
    {
      if (!_hovered) return;

      var sign = Mathf.Sign(context.ReadValue<Vector2>().x);
      var time = _videoPlayerController.ClockTime + sign * ConfigManager.Config.skipLength;
      time = Math.Max(Math.Min(time, _videoPlayerController.Length), 0);
      SetVideoTime(time);
      if (!_videoPlayerController.IsPlaying)
      {
        UpdateProgressIndicator(time);
        UpdateText(time);
      }

      DresClientManager.LogInteraction("videoPlayer", $"skipped {_mediaObject.Id} {time} {sign}");
    }

    private void Update()
    {
      if (_videoPlayerController is {IsPlaying: true})
      {
        var time = _videoPlayerController.ClockTime;
        UpdateProgressIndicator(time);
        UpdateText(time);
      }
    }

    public void OnClick(PointerEventData pointerEventData)
    {
      if (_videoPlayerController.IsPlaying)
      {
        _videoPlayerController.Pause();
        DresClientManager.LogInteraction("videoPlayer", $"pause {_mediaObject.Id} {_videoPlayerController.ClockTime}");
      }
      else
      {
        _videoPlayerController.Play();
        DresClientManager.LogInteraction("videoPlayer", $"play {_mediaObject.Id} {_videoPlayerController.ClockTime}");
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

      SetVideoTime(newTime);

      UpdateProgressIndicator(newTime);
      UpdateText(newTime);
      DresClientManager.LogInteraction("videoPlayer", $"jump {_mediaObject.Id} {newTime}");
    }

    private void SetVideoTime(double time)
    {
      UpdateProgressIndicator(time);
      if (_videoPlayerController.IsPlaying)
      {
        _videoPlayerController.Pause();
        _videoPlayerController.SetTime(time);
        _videoPlayerController.Play();
      }
      else
      {
        _videoPlayerController.SetTime(time);
      }
    }

    private async void SkipToSegment(int segmentIndex)
    {
      var segmentStart = await _segments[segmentIndex].GetAbsoluteStart();
      SetVideoTime(segmentStart);
    }

    private async void PrepareCompleted(RenderTexture texture)
    {
      // Get video dimensions and scale preview image to fit video into 1x1 square
      var width = _videoPlayerController.Width;
      var height = _videoPlayerController.Height;
      var factor = Mathf.Max(width, height);
      previewImage.texture = texture;
      _imageTransform.sizeDelta = new Vector2(1000f * width / factor, 1000f * height / factor);

      var start = await _segment.GetAbsoluteStart();
      var end = await _segment.GetAbsoluteEnd();
      var length = _videoPlayerController.Length;
      UpdateProgressIndicator(start);
      SetSegmentIndicator(start, end, length, segmentIndicator);
      // Set progress bar active
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
      var progressTexture = new Texture2D(1000, 1);
      var colors = new Color[1000];
      var backgroundColor = new Color(.39f, .39f, .39f);
      for (var j = 0; j < colors.Length; j++)
      {
        colors[j] = backgroundColor;
      }

      progressTexture.SetPixels(0, 0, 1000, 1, colors);
      progressTexture.Apply();
      progressBar.GetComponent<RawImage>().texture = progressTexture;
      var i = 0;
      foreach (var segStart in segmentStarts)
      {
        progressTexture.SetPixel((int) (999 * (segStart / _videoPlayerController.Length)), 0, Color.black);
        i++;
        if (i == InstantiationBatch)
        {
          i = 0;
          progressTexture.Apply();
          yield return null;
        }
      }

      progressTexture.Apply();
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
      var current = TimeSpan.FromSeconds(time).ToString("g");
      var segment = await GetCurrentSegment(time);
      var segId = segment != null ? segment.Id : "Unknown";
      segmentDataText.text = $"{mediaObjectId}: {current}\nCurrent: {segId}";
    }

    private async Task<SegmentData> GetCurrentSegment(double time)
    {
      foreach (var segment in _segments)
      {
        var start = await segment.GetAbsoluteStart();
        var end = await segment.GetAbsoluteEnd();

        if (start <= time && time <= end)
        {
          return segment;
        }
      }

      return null;
    }

    private void SetSegmentIndicator(double start, double end, double length, RectTransform rt)
    {
      var rect = progressBar.rect;
      rt.anchoredPosition = new Vector2((float) (rect.width * start / length), 0);
      rt.sizeDelta = new Vector2((float) (rect.width * (end - start) / length), 0);
    }
  }
}
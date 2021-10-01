using System;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Config;
using VitrivrVR.Notification;
using VitrivrVR.Submission;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Display
{
  public class CanvasImageSequenceDisplay : MediaDisplay
  {
    public Texture2D errorTexture;
    public RawImage previewImage;
    public GameObject metadataButton;
    public GameObject submitButton;
    public Transform bottomStack;
    public TextMeshProUGUI segmentDataText;

    public GameObject scrollableUITablePrefab;
    public ScrollRect scrollableListPrefab;
    public GameObject listItemPrefab;
    public GameObject mediaObjectSegmentViewPrefab;

    /// <summary>
    /// The number of neighbors to show in the segment view.
    /// </summary>
    private const int MAXNeighbors = 200;

    private ScoredSegment _scoredSegment;
    private SegmentData Segment => _scoredSegment.segment;
    private ObjectData _mediaObject;
    private Action _onClose;

    private bool _metadataShown;
    private GameObject _objectSegmentView;

    public override async void Initialize(ScoredSegment scoredSegment, Action onClose)
    {
      _scoredSegment = scoredSegment;
      _onClose = onClose;
      _mediaObject = ObjectRegistry.GetObject(await Segment.GetObjectId());

      var sn = await Segment.GetSequenceNumber();
      segmentDataText.text = $"{Segment.Id}:\nNumber: {sn}\nScore: {_scoredSegment.score:F}";

      // Resolve media URL
      var mediaUrl = await CineastWrapper.GetMediaUrlOfAsync(_mediaObject, Segment.Id);

      StartCoroutine(DownloadHelper.DownloadTexture(mediaUrl,
        () => { previewImage.texture = errorTexture; },
        texture =>
        {
          previewImage.texture = texture;
          var width = texture.width;
          var height = texture.height;
          var factor = Mathf.Max(width, height);
          var imageTransform = previewImage.GetComponent<RectTransform>();
          imageTransform.sizeDelta = new Vector2(1000f * width / factor, 1000f * height / factor);
        }
      ));

      // Enable DRES submission button
      if (ConfigManager.Config.dresEnabled)
      {
        submitButton.SetActive(true);
        DresClientManager.LogInteraction("imageSequenceDisplay", $"initialized {_mediaObject.Id} {Segment.Id}");
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

      DresClientManager.LogInteraction("imageSequenceDisplay", $"closed {_mediaObject.Id} {Segment.Id}");
    }

    public async void ShowMetadata()
    {
      if (_metadataShown)
      {
        return;
      }

      _metadataShown = true;
      Destroy(metadataButton);

      // Segment tags
      var tagList = Instantiate(scrollableListPrefab, bottomStack);
      var listRect = tagList.GetComponent<RectTransform>();
      listRect.anchorMin = new Vector2(0, .5f);
      listRect.anchorMax = new Vector2(0, .5f);
      listRect.sizeDelta = new Vector2(100, 600);

      var listContent = tagList.content;

      // TODO: Preload or cache for all results
      var tagIds = await CineastWrapper.MetadataApi.FindTagsByIdAsync(Segment.Id);

      var tags = await CineastWrapper.TagApi.FindTagsByIdAsync(new IdList(tagIds.TagIDs));

      foreach (var tagData in tags.Tags)
      {
        var tagItem = Instantiate(listItemPrefab, listContent);
        tagItem.GetComponentInChildren<TextMeshProUGUI>().text = tagData.Name;
      }

      DresClientManager.LogInteraction("segmentTags", $"opened {_mediaObject.Id} {Segment.Id}");
    }

    public async void ShowObjectSegmentView()
    {
      if (_objectSegmentView)
      {
        Destroy(_objectSegmentView);
      }
      else
      {
        var index = await Segment.GetSequenceNumber();
        var min = index - MAXNeighbors;
        var max = index + MAXNeighbors;
        var t = transform;
        _objectSegmentView = Instantiate(mediaObjectSegmentViewPrefab, t.position - 0.2f * t.forward, t.rotation);
        _objectSegmentView.GetComponentInChildren<MediaObjectSegmentView>()
          .Initialize(_mediaObject, OpenSegment, min, max);
      }
    }

    public void Submit()
    {
      if (!ConfigManager.Config.dresEnabled)
      {
        NotificationController.Notify("Dres is disabled!");
        return;
      }

      DresClientManager.SubmitResult(Segment.Id);
    }

    private async void OpenSegment(int segmentIndex)
    {
      // TODO: Refactor to avoid having to fetch and initialize all segments of given object
      var segments = await SegmentRegistry.GetSegmentsOf(_mediaObject.Id);
      await SegmentRegistry.BatchFetchSegmentData(segments);
      segments = segments.Where(segment => segment.GetSequenceNumber().Result == segmentIndex).ToList();

      if (segments.Count != 1)
      {
        Debug.LogError($"Unexpected number of segments found with sequence number {segmentIndex}: {segments.Count}");
      }

      var segment = segments.First();
      var scoredSegment = new ScoredSegment(segment, 0);
      var t = _objectSegmentView != null ? _objectSegmentView.transform : transform;
      await MediaDisplayFactory.CreateDisplay(scoredSegment, () => { }, t.position + 0.2f * t.up, t.rotation);
    }
  }
}
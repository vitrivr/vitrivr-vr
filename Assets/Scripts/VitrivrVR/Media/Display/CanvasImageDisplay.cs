﻿using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Logging;
using VitrivrVR.Notification;
using VitrivrVR.Submission;
using VitrivrVR.UI;
using VitrivrVR.Util;
using static VitrivrVR.Logging.Interaction;

namespace VitrivrVR.Media.Display
{
  public class CanvasImageDisplay : MediaDisplay
  {
    public Texture2D errorTexture;
    public RawImage previewImage;
    public GameObject submitButton;
    public Transform bottomStack;
    public TextMeshProUGUI segmentDataText;

    public GameObject scrollableUITablePrefab;
    public ScrollRect scrollableListPrefab;
    public GameObject listItemPrefab;

    private ScoredSegment _scoredSegment;
    private SegmentData Segment => _scoredSegment.segment;
    private ObjectData _mediaObject;
    private Action _onClose;

    private bool _metadataShown;
    private GameObject _metadataTable;
    private bool _tagListShown;
    private ScrollRect _tagList;

    public override async void Initialize(ScoredSegment scoredSegment, Action onClose)
    {
      _scoredSegment = scoredSegment;
      _onClose = onClose;
      _mediaObject = await Segment.GetObject();

      segmentDataText.text = $"{_mediaObject.Id}:\nScore: {_scoredSegment.score:F}";
      LayoutRebuilder.ForceRebuildLayoutImmediate(segmentDataText.rectTransform.parent as RectTransform);
      segmentDataText.rectTransform.sizeDelta = segmentDataText.GetPreferredValues();

      // Resolve media URL
      var mediaUrl = await _mediaObject.GetMediaUrl();

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
      }

      LoggingController.LogInteraction("imageSequenceDisplay", $"initialized {_mediaObject.Id} {Segment.Id}",
        ResultExpansion);
    }

    public void Close()
    {
      Destroy(gameObject);
      _onClose();
    }

    private void OnDestroy()
    {
      LoggingController.LogInteraction("imageDisplay", $"closed {_mediaObject.Id} {Segment.Id}", Other);
    }

    public async void ToggleMetadata()
    {
      if (_metadataShown)
      {
        Destroy(_metadataTable);
        _metadataShown = false;
        LoggingController.LogInteraction("mediaSegmentMetadata", $"closed {_mediaObject.Id}", Other);
        return;
      }

      _metadataShown = true;

      var metadata = await Segment.GetMetadata();
      var rows = metadata.Values.Select(domain => domain.Count).Aggregate(0, (x, y) => x + y);
      var table = new string[rows, 3];
      var i = 0;
      foreach (var (domain, pairs) in metadata.Where(domain => domain.Value.Count != 0))
      {
        // Fill first column
        table[i, 0] = domain;
        for (var j = 1; j < pairs.Count; j++)
        {
          table[i + j, 0] = "";
        }

        // Fill key-value pairs
        foreach (var ((key, value), index) in pairs.Select((pair, index) => (pair, index)))
        {
          table[i + index, 1] = key;
          table[i + index, 2] = value;
        }

        i += pairs.Count;
      }

      _metadataTable = Instantiate(scrollableUITablePrefab, bottomStack);
      var uiTableController = _metadataTable.GetComponentInChildren<UITableController>();
      uiTableController.table = table;
      var uiTableTransform = _metadataTable.GetComponent<RectTransform>();
      uiTableTransform.sizeDelta = new Vector2(100, 600); // x is completely irrelevant here, since width is auto

      LoggingController.LogInteraction("mediaObjectMetadata", $"opened {_mediaObject.Id}", ResultExpansion);
    }

    public async void ToggleTagList()
    {
      if (_tagListShown)
      {
        Destroy(_tagList.gameObject);
        _tagListShown = false;
        LoggingController.LogInteraction("segmentTags", $"closed {_mediaObject.Id}", Other);
        return;
      }

      _tagListShown = true;

      _tagList = Instantiate(scrollableListPrefab, bottomStack);
      var listRect = _tagList.GetComponent<RectTransform>();
      listRect.anchorMin = new Vector2(0, .5f);
      listRect.anchorMax = new Vector2(0, .5f);
      listRect.sizeDelta = new Vector2(100, 600);

      var listContent = _tagList.content;

      var tags = await Segment.GetTags();

      foreach (var tagData in tags)
      {
        var tagItem = Instantiate(listItemPrefab, listContent);
        tagItem.GetComponentInChildren<TextMeshProUGUI>().text = tagData.Name;
      }

      LoggingController.LogInteraction("segmentTags", $"opened {_mediaObject.Id} {Segment.Id}", ResultExpansion);
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

    private void Awake()
    {
      GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }
  }
}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Interaction.System;
using VitrivrVR.Interaction.System.Grab;
using VitrivrVR.Logging;
using VitrivrVR.Media.Controller;
using static VitrivrVR.Logging.Interaction;

namespace VitrivrVR.Media.Display
{
  public class MediaObjectSegmentView : Grabable
  {
    public ThumbnailController thumbnailPrefab;
    public Transform root;
    public Transform grabHandle;

    private ThumbnailController[] _thumbnails;
    private Action<int, Vector3> _onSegmentSelection;
    private ObjectData _mediaObject;

    /// <summary>
    /// The minimum segment index to display as thumbnail.
    /// </summary>
    private int _minIndex;

    /// <summary>
    /// Only every index-factor-th segment thumbnail should be displayed to improve loading and visibility.
    /// </summary>
    private int _indexFactor;

    /// <summary>
    /// The maximum number of thumbnails to display, otherwise skips thumbnails through index factor.
    /// </summary>
    private const int MaxThumbnails = 400;

    private readonly Dictionary<Interactor, int> _enteredInteractors = new();

    /// <summary>
    /// Number of segment thumbnails to instantiate each frame in Coroutine.
    /// </summary>
    private const int InstantiationBatch = 100;

    private new void Awake()
    {
      grabTransform = root;
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.TryGetComponent<Interactor>(out var interactor))
      {
        _enteredInteractors.Add(interactor, -1);
        LoggingController.LogInteraction("videoSummary", $"browse started {_mediaObject.Id} {interactor.name}",
          Browsing);
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.TryGetComponent<Interactor>(out var interactor))
      {
        var index = _enteredInteractors[interactor];
        if (index != -1)
        {
          SetThumbnailHeight(index, false);
        }

        _enteredInteractors.Remove(interactor);
        LoggingController.LogInteraction("videoSummary", $"browse stopped {_mediaObject.Id} {interactor.name}",
          Browsing);
      }
    }

    private new void Update()
    {
      base.Update();

      var t = transform.parent;
      // Scale according to grab handle
      var scale = t.localScale;
      scale.z = 1 - grabHandle.localPosition.z;
      t.localScale = scale;

      foreach (var index in _enteredInteractors.Values.Where(index => index != -1))
      {
        SetThumbnailHeight(index, false);
      }

      foreach (var other in _enteredInteractors.Keys.ToList())
      {
        var index = GetSegmentIndex(other.transform);

        _enteredInteractors[other] = index;
        SetThumbnailHeight(index, true);
      }
    }

    private void OnDestroy()
    {
      LoggingController.LogInteraction("videoSummary", $"closed {_mediaObject.Id}", Other);
    }

    public async void Initialize(ObjectData mediaObject, Action<int, Vector3> onSegmentSelection, int min = 0,
      int max = -1)
    {
      _onSegmentSelection = onSegmentSelection;
      _mediaObject = mediaObject;

      var segments = await mediaObject.GetSegments();

      var segmentInfo =
        await Task.WhenAll(segments.Select(async segment =>
          (segment, index: await segment.GetSequenceNumber())));

      // Filter to specified segment range
      if (max > min)
      {
        segmentInfo = segmentInfo.Where(item => min <= item.index && item.index <= max).ToArray();
      }

      // Make sure segments are unique (this may be removed if it can be guaranteed on server side)
      segmentInfo = segmentInfo.Distinct().ToArray();
      _minIndex = segmentInfo.Select(item => item.index).Min();

      // Check if there are too many segments
      _indexFactor = Mathf.CeilToInt((float) segmentInfo.Length / MaxThumbnails);
      if (_indexFactor > 1)
      {
        segmentInfo = segmentInfo.Where((x, i) => i % _indexFactor == 0).ToArray();
      }

      _thumbnails = new ThumbnailController[segmentInfo.Length];
      StartCoroutine(InstantiateSegmentIndicators(segmentInfo, segmentInfo.Length));


      // TODO: Translate type in DresClientManager to support other media object types
      LoggingController.LogInteraction("videoSummary", $"initialized {_mediaObject.Id}", ResultExpansion);
    }

    public override void OnInteraction(Transform interactor, bool start)
    {
      if (start) return;
      var rawIndex = GetSegmentIndex(interactor);
      // Adjust for index factor
      rawIndex *= _indexFactor;
      var segmentIndex = rawIndex + _minIndex - 1;
      _onSegmentSelection(segmentIndex, interactor.position);
      LoggingController.LogInteraction("videoSummary", $"selected {_mediaObject.Id} {segmentIndex} {interactor.name}",
        ResultExpansion);
    }

    private IEnumerator InstantiateSegmentIndicators(IEnumerable<(SegmentData segment, int seqNum)> segmentInfo,
      int numSegments)
    {
      var i = 0;
      foreach (var (segment, seqNum) in segmentInfo)
      {
        var thumbnailUrl = segment.GetThumbnailUrl().Result;

        var thumbnail = Instantiate(thumbnailPrefab, transform);
        thumbnail.url = thumbnailUrl;

        thumbnail.transform.localPosition = Vector3.forward * ((float) (seqNum - _minIndex) / _indexFactor / numSegments - 0.5f);

        _thumbnails[(seqNum - _minIndex) / _indexFactor] = thumbnail;

        i++;
        if (i == InstantiationBatch)
        {
          i = 0;
          yield return null;
        }
      }

      VerifyMediaSegments();
    }

    /// <summary>
    /// Verifies that the segment array is filled with instantiated segment prefabs.
    /// 
    /// In case not all array elements are correctly filled, an error message with debug information is printed.
    /// </summary>
    private void VerifyMediaSegments()
    {
      var missing = new List<int>();
      for (var i = 0; i < _thumbnails.Length; i++)
      {
        var thumbnail = _thumbnails[i];
        if (thumbnail == null)
        {
          missing.Add(i * _indexFactor + _minIndex + 1);
        }
      }

      if (missing.Count > 0)
      {
        var id = _mediaObject.Id;
        var missingIndexes = string.Join(", ", missing);
        Debug.LogError($"Some media object segments were not correctly instantiated for {id}." +
                       $"\nMissing sequence numbers: {missingIndexes}");
      }
    }

    private void SetThumbnailHeight(int index, bool selected)
    {
      var thumbnail = _thumbnails[index];
      var thumbnailTransform = thumbnail.transform;
      var position = thumbnailTransform.localPosition;
      position.y = selected ? thumbnailTransform.localScale.y : 0;
      thumbnailTransform.localPosition = position;
    }

    private int GetSegmentIndex(Transform other)
    {
      var otherTransform = transform.InverseTransformPoint(other.position);
      return (int) Mathf.Min(Mathf.Max((otherTransform.z + 0.5f) * _thumbnails.Length, 0), _thumbnails.Length - 1);
    }
  }
}
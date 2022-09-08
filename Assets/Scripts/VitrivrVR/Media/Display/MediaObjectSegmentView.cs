using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Interaction.System;
using VitrivrVR.Interaction.System.Grab;
using VitrivrVR.Media.Controller;
using VitrivrVR.Submission;

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

    private int _minIndex;

    private readonly Dictionary<Interactor, int> _enteredInteractors = new Dictionary<Interactor, int>();

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
        DresClientManager.LogInteraction("videoSummary", $"browse started {_mediaObject.Id}");
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
        DresClientManager.LogInteraction("videoSummary", $"browse stopped {_mediaObject.Id}");
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
      DresClientManager.LogInteraction("videoSummary", $"closed {_mediaObject.Id}");
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

      _thumbnails = new ThumbnailController[segmentInfo.Length];
      StartCoroutine(InstantiateSegmentIndicators(segmentInfo, segmentInfo.Length));

      // TODO: Translate type in DresClientManager to support other media object types
      DresClientManager.LogInteraction("videoSummary", $"initialized {_mediaObject.Id}");
    }

    public override void OnInteraction(Transform interactor, bool start)
    {
      if (start) return;
      var rawIndex = GetSegmentIndex(interactor);
      var segmentIndex = rawIndex + _minIndex - 1;
      _onSegmentSelection(segmentIndex, interactor.position);
      DresClientManager.LogInteraction("videoSummary", $"selected {_mediaObject.Id} {segmentIndex}");
    }

    private IEnumerator InstantiateSegmentIndicators(IEnumerable<(SegmentData segment, int seqNum)> segmentInfo,
      int numSegments)
    {
      var i = 0;
      foreach (var (segment, seqNum) in segmentInfo)
      {
        var thumbnailUrl = CineastWrapper.GetThumbnailUrlOf(segment);

        var thumbnail = Instantiate(thumbnailPrefab, transform);
        thumbnail.url = thumbnailUrl;

        thumbnail.transform.localPosition = Vector3.forward * ((float) (seqNum - _minIndex) / numSegments - 0.5f);

        _thumbnails[seqNum - _minIndex] = thumbnail;

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
          missing.Add(i + _minIndex + 1);
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
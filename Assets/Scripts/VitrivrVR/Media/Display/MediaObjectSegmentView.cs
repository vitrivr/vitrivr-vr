using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Interaction.System;
using VitrivrVR.Media.Controller;

namespace VitrivrVR.Media.Display
{
  public class MediaObjectSegmentView : Interactable
  {
    public ThumbnailController thumbnailPrefab;
    public Transform root;
    public Transform grabHandle;

    private ThumbnailController[] _thumbnails;
    private Action<int> _onSegmentSelection;

    /// <summary>
    /// Store reference of grabbing transform while grabbed
    /// </summary>
    private Transform _grabber;

    private Vector3 _grabAnchor;

    private readonly Dictionary<Interactor, int> _enteredInteractors = new Dictionary<Interactor, int>();

    /// <summary>
    /// Number of segment thumbnails to instantiate each frame in Coroutine.
    /// </summary>
    private const int InstantiationBatch = 100;

    private void OnTriggerEnter(Collider other)
    {
      if (other.TryGetComponent<Interactor>(out var interactor))
      {
        _enteredInteractors.Add(interactor, -1);
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
      }
    }

    private void Update()
    {
      var t = transform.parent;
      // Scale according to grab handle
      var scale = t.localScale;
      scale.z = 1 - grabHandle.localPosition.z;
      t.localScale = scale;

      // Move if grabbed
      if (_grabber)
      {
        root.localPosition = root.parent.InverseTransformPoint(_grabber.position) + _grabAnchor;
      }

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

    public async void Initialize(ObjectData mediaObject, Action<int> onSegmentSelection)
    {
      _onSegmentSelection = onSegmentSelection;

      var segments = await mediaObject.GetSegments();
      _thumbnails = new ThumbnailController[segments.Count];

      var segmentInfo =
        await Task.WhenAll(segments.Select(async segment => (segment, await segment.GetSequenceNumber() - 1)));

      StartCoroutine(InstantiateSegmentIndicators(segmentInfo, segments.Count));
    }

    public override void OnInteraction(Transform interactor, bool start)
    {
      if (!start) return;
      var segmentIndex = GetSegmentIndex(interactor);
      _onSegmentSelection(segmentIndex);
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      _grabber = start ? interactor : null;
      if (start)
      {
        _grabAnchor = root.localPosition - root.parent.InverseTransformPoint(interactor.position);
      }
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

        thumbnail.transform.localPosition = Vector3.forward * ((float) seqNum / numSegments - 0.5f);

        _thumbnails[seqNum] = thumbnail;

        i++;
        if (i == InstantiationBatch)
        {
          i = 0;
          yield return null;
        }
      }
    }

    private void SetThumbnailHeight(int index, bool selected)
    {
      var thumbnailTransform = _thumbnails[index].transform;
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
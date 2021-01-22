using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;

namespace VitrivrVR.Media
{
  public class MediaObjectSegmentView : MonoBehaviour
  {
    public ThumbnailController thumbnailPrefab;

    private ObjectData _mediaObject;
    private ThumbnailController[] _thumbnails;

    private readonly Dictionary<Collider, int> _enteredColliders = new Dictionary<Collider, int>();

    /// <summary>
    /// Number of segment thumbnails to instantiate each frame in Coroutine.
    /// </summary>
    private const int InstantiationBatch = 100;

    private void OnTriggerEnter(Collider other)
    {
      _enteredColliders.Add(other, -1);
    }

    private void OnTriggerExit(Collider other)
    {
      var index = _enteredColliders[other];
      if (index != -1)
      {
        SetThumbnailHeight(index, false);
      }

      _enteredColliders.Remove(other);
    }

    private void FixedUpdate()
    {
      foreach (var index in _enteredColliders.Values.Where(index => index != -1))
      {
        SetThumbnailHeight(index, false);
      }

      foreach (var other in _enteredColliders.Keys.ToList())
      {
        var otherTransform = transform.InverseTransformPoint(other.transform.position);
        var index = (int) Mathf.Min(Mathf.Max((otherTransform.z + 0.5f) * _thumbnails.Length, 0),
          _thumbnails.Length - 1);

        _enteredColliders[other] = index;
        SetThumbnailHeight(index, true);
      }
    }

    public async void Initialize(ObjectData mediaObject)
    {
      _mediaObject = mediaObject;

      var segments = await mediaObject.GetSegments();
      _thumbnails = new ThumbnailController[segments.Count];

      var segmentInfo =
        await Task.WhenAll(segments.Select(async segment => (segment.Id, await segment.GetSequenceNumber() - 1)));

      StartCoroutine(InstantiateSegmentIndicators(segmentInfo, segments.Count));
    }

    private IEnumerator InstantiateSegmentIndicators(IEnumerable<(string segId, int seqNum)> segmentInfo,
      int numSegments)
    {
      var i = 0;
      var config = CineastConfigManager.Instance.Config;

      foreach (var (segId, seqNum) in segmentInfo)
      {
        var thumbnailPath = PathResolver.ResolvePath(config.thumbnailPath, _mediaObject.Id, segId);
        var thumbnailUrl = $"{config.mediaHost}{thumbnailPath}{config.thumbnailExtension}";

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
  }
}
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

    private int _selectedIndex = -1;

    private void OnTriggerStay(Collider other)
    {
      var localOther = transform.InverseTransformPoint(other.transform.position);
      var index = (int) Mathf.Min(Mathf.Max((localOther.z + 0.5f) * _thumbnails.Length, 0), _thumbnails.Length - 1);

      if (index != _selectedIndex)
      {
        SetThumbnailHeight(index, true);
        if (_selectedIndex != -1)
        {
          SetThumbnailHeight(_selectedIndex, false);
        }

        _selectedIndex = index;
      }
    }

    private void OnTriggerExit(Collider _)
    {
      if (_selectedIndex != -1)
      {
        SetThumbnailHeight(_selectedIndex, false);
        _selectedIndex = -1;
      }
    }

    public async void Initialize(ObjectData mediaObject)
    {
      _mediaObject = mediaObject;

      var segments = await mediaObject.GetSegments();
      _thumbnails = new ThumbnailController[segments.Count];

      var config = CineastConfigManager.Instance.Config;

      foreach (var segment in segments)
      {
        var thumbnailPath = PathResolver.ResolvePath(config.thumbnailPath, _mediaObject.Id, segment.Id);
        var thumbnailUrl = $"{config.mediaHost}{thumbnailPath}{config.thumbnailExtension}";

        var i = await segment.GetSequenceNumber() - 1;

        var thumbnail = Instantiate(thumbnailPrefab, transform);
        thumbnail.url = thumbnailUrl;

        thumbnail.transform.localPosition = Vector3.forward * ((float) i / segments.Count - 0.5f);

        _thumbnails[i] = thumbnail;
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
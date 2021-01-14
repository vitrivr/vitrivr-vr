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

        thumbnail.transform.localPosition = Vector3.back * (1 - (float) i / segments.Count + 0.5f);

        _thumbnails[i] = thumbnail;
      }
    }
  }
}
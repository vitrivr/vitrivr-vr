using System;
using System.Collections.Generic;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

public class MediaCarouselController : MonoBehaviour
{
  public ThumbnailController thumbnailTemplate;
  public int rows = 3;
  public string scrollAxis = "Horizontal";
  public float scrollSpeed = 2f;
  
  private List<GameObject> _thumbnails = new List<GameObject>();
  private string _thumbnailPath;
  private string _thumbnailExtension;
  private string _mediaHost;

  public void Awake()
  {
    var config = CineastConfigManager.Instance.Config;
    _thumbnailPath = config.thumbnailPath;
    _thumbnailExtension = config.thumbnailExtension;
    _mediaHost = config.mediaHost;
  }

  void Update()
  {
    var scroll = Input.GetAxisRaw(scrollAxis);
    transform.Translate(Time.deltaTime * scrollSpeed * scroll * Vector3.left);
  }

  public void CreateResults(List<MediaSegmentDescriptor> queryResults)
  {
    foreach (var result in queryResults)
    {
      var thumbnailController = Instantiate(thumbnailTemplate, new Vector3(_thumbnails.Count / rows, rows - 1 -
        _thumbnails.Count % rows), Quaternion.identity, transform);
      var thumbnailPath =
        PathResolver.ResolvePath(_thumbnailPath, result.ObjectId, result.SegmentId);
      thumbnailController.URL = $"{_mediaHost}{thumbnailPath}{_thumbnailExtension}";
      _thumbnails.Add(thumbnailController.gameObject);
    }
  }
  
  public void ClearResults()
  {
    foreach (var thumbnail in _thumbnails)
    {
      Destroy(thumbnail);
    }

    transform.position = Vector3.zero;

    _thumbnails.Clear();
  }
}

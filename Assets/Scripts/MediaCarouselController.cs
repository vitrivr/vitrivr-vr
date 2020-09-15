using System.Collections.Generic;
using System.Linq;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

public class MediaCarouselController : MonoBehaviour
{
  public ThumbnailController thumbnailTemplate;
  public int rows = 3;
  public float innerRadius = 2.7f;
  public string scrollAxis = "Horizontal";
  public float scrollSpeed = 30f;
  public float forceThreshold = 0.3f; // Threshold after which forces moving thumbnails apart are no longer applied

  private readonly List<(GameObject thumbnail, float score)> _thumbnails = new List<(GameObject, float)>();
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
    // Rotate carousel
    var scroll = Input.GetAxisRaw(scrollAxis);
    Transform transform1 = transform;
    transform1.Rotate(Vector3.up, Time.deltaTime * scrollSpeed * scroll);
    
    // Move thumbnails to create more organic spacing
    var targetPosition = transform1.position;

    foreach (var (thumbnail, score) in _thumbnails)
    {
      // Force pulling thumbnail to designated distance from center
      var sqrTargetDistance = Mathf.Pow(innerRadius + 1 - score, 2);
      var displacement = targetPosition - thumbnail.transform.position;
      var sqrDistance = displacement.sqrMagnitude;
      var force = displacement.normalized * (sqrDistance - sqrTargetDistance);
      // Forces pushing thumbnail away from other thumbnails
      foreach (var (other, _) in _thumbnails)
      {
        if (thumbnail == other)
        {
          continue;
        }

        var neighborDisplacement = thumbnail.transform.position - other.transform.position;

        if (neighborDisplacement.sqrMagnitude < 2)
        {
          force += neighborDisplacement / neighborDisplacement.sqrMagnitude;
        }
      }

      // Apply forces immediately to avoid getting stuck in local minima
      if (force.sqrMagnitude > forceThreshold)
      {
        thumbnail.transform.position += force * Time.deltaTime;
      }
      
      // Rotate thumbnail to face center
      thumbnail.transform.rotation = Quaternion.LookRotation(thumbnail.transform.position - targetPosition, Vector3.up);
    }
  }

  public void CreateResults(List<(MediaSegmentDescriptor item, double score)> queryResults)
  {
    foreach (var result in queryResults)
    {
      CreateResultObject(result);
    }
  }

  private void CreateResultObject((MediaSegmentDescriptor item, double score) result)
  {
    var angle = 30; // Angle between thumbnails
    // Determine position
    var position = new Vector3(0, 0, innerRadius + 1 - (float) result.score + Mathf.Floor(_thumbnails.Count / (360f / angle)));
    position = Quaternion.Euler((_thumbnails.Count % rows - (rows - 1) / 2) * angle, _thumbnails.Count / rows * angle,
      0) * position;
    var targetPosition = transform.position;
    position += targetPosition; // Move result display focus a little bit higher
    // Rotate thumbnail to face center
    var rotation = Quaternion.LookRotation(position - targetPosition, Vector3.up);
    var thumbnailController = Instantiate(thumbnailTemplate, position, rotation, transform);
    var thumbnailPath =
      PathResolver.ResolvePath(_thumbnailPath, result.item.ObjectId, result.item.SegmentId);
    thumbnailController.URL = $"{_mediaHost}{thumbnailPath}{_thumbnailExtension}";
    _thumbnails.Add((thumbnailController.gameObject, (float) result.score));
  }

  public void ClearResults()
  {
    // Destroy all thumbnails
    foreach (var (thumbnail, _) in _thumbnails)
    {
      Destroy(thumbnail);
    }

    // Reset rotation
    transform.rotation = Quaternion.identity;

    _thumbnails.Clear();
  }
}
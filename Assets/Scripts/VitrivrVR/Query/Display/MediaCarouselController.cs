using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using VitrivrVR.Media;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Very simple <see cref="QueryDisplay"/> showing results in a spherical manner around the user.
  /// </summary>
  public class MediaCarouselController : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public int rows = 3;
    public float innerRadius = 2.7f;
    public float itemAngle = 30; // Angle between media items
    public string scrollAxis = "Horizontal";
    public float scrollSpeed = 30f;
    public float forceThreshold = 0.3f; // Threshold after which forces moving thumbnails apart are no longer applied
    public int maxResults = 72;

    private readonly List<(GameObject thumbnail, float score)> _thumbnails = new List<(GameObject, float)>();

    private void Update()
    {
      // Rotate carousel
      var scroll = UnityEngine.Input.GetAxisRaw(scrollAxis);
      var transform1 = transform;
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
        thumbnail.transform.rotation =
          Quaternion.LookRotation(thumbnail.transform.position - targetPosition, Vector3.up);
      }
    }

    private async void CreateResults(QueryData query)
    {
      // TODO: Turn this into a query display factory and separate query display object
      // TODO: Proper result merging
      var tasks = query.results.Values
        .Aggregate((IEnumerable<(SegmentData item, double score)>) new List<(SegmentData item, double score)>(),
          (collection, categoryList) => collection.Concat(categoryList)).Take(maxResults).ToList()
        .Select(CreateResultObject);
      await Task.WhenAll(tasks);
    }

    private async Task CreateResultObject((SegmentData item, double score) result)
    {
      var itemDisplay = Instantiate(mediaItemDisplay, transform);
      await itemDisplay.Initialize(result.item);

      // Determine position
      var position = new Vector3(0, 0,
        innerRadius + 1 - (float) result.score + Mathf.Floor(_thumbnails.Count / (360f / itemAngle)));
      position = Quaternion.Euler((_thumbnails.Count % rows - (rows - 1) / 2) * itemAngle,
        _thumbnails.Count / rows * itemAngle, 0) * position;
      var targetPosition = transform.position;
      position += targetPosition; // Move result display focus a little bit higher
      // Rotate thumbnail to face center
      var rotation = Quaternion.LookRotation(position - targetPosition, Vector3.up);

      var itemTransform = itemDisplay.transform;
      itemTransform.position = position;
      itemTransform.rotation = rotation;

      _thumbnails.Add((itemDisplay.gameObject, (float) result.score));
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

    public override void Initialize(QueryData queryData)
    {
      CreateResults(queryData);
    }

    private void OnDestroy()
    {
      ClearResults();
    }
  }
}
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

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays = new List<(MediaItemDisplay, float)>();

    private void Update()
    {
      // Rotate carousel
      var scroll = UnityEngine.Input.GetAxisRaw(scrollAxis);
      var transform1 = transform;
      transform1.Rotate(Vector3.up, Time.deltaTime * scrollSpeed * scroll);

      // Move thumbnails to create more organic spacing
      var targetPosition = transform1.position;

      foreach (var (display, score) in _mediaDisplays)
      {
        // Force pulling thumbnail to designated distance from center
        var sqrTargetDistance = Mathf.Pow(innerRadius + 1 - score, 2);
        var displacement = targetPosition - display.transform.position;
        var sqrDistance = displacement.sqrMagnitude;
        var force = displacement.normalized * (sqrDistance - sqrTargetDistance);
        // Forces pushing thumbnail away from other thumbnails
        foreach (var (other, _) in _mediaDisplays)
        {
          if (display == other)
          {
            continue;
          }

          var neighborDisplacement = display.transform.position - other.transform.position;

          if (neighborDisplacement.sqrMagnitude < 2)
          {
            force += neighborDisplacement / neighborDisplacement.sqrMagnitude;
          }
        }

        // Apply forces immediately to avoid getting stuck in local minima
        if (force.sqrMagnitude > forceThreshold)
        {
          display.transform.position += force * Time.deltaTime;
        }

        // Rotate media display to face center
        display.transform.rotation =
          Quaternion.LookRotation(display.transform.position - targetPosition);
      }
    }

    private async void CreateResults(QueryData query)
    {
      // TODO: Turn this into a query display factory and separate query display object
      var fusionResults = query.GetMeanFusionResults();
      var tasks = fusionResults
        .Take(maxResults)
        .Select(CreateResultObject);
      await Task.WhenAll(tasks);
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      var itemDisplay = Instantiate(mediaItemDisplay, transform);

      // Determine position
      var position = new Vector3(0, 0,
        innerRadius + 1 - (float) result.score + Mathf.Floor(_mediaDisplays.Count / (360f / itemAngle)));
      var column = _mediaDisplays.Count % rows - (rows - 1) / 2;
      var row = _mediaDisplays.Count / rows;
      position = Quaternion.Euler(column * itemAngle, row * itemAngle, 0) * position;
      var targetPosition = transform.position;
      position += targetPosition; // Move result display focus a little bit higher
      // Rotate media display to face center
      var rotation = Quaternion.LookRotation(position - targetPosition, Vector3.up);

      var itemTransform = itemDisplay.transform;
      itemTransform.position = position;
      itemTransform.rotation = rotation;

      _mediaDisplays.Add((itemDisplay, (float) result.score));
      
      // Only begin initialization after determining position so that results can begin positioning
      await itemDisplay.Initialize(result.segment);
    }

    public void ClearResults()
    {
      // Destroy all media displays
      foreach (var (display, _) in _mediaDisplays)
      {
        Destroy(display.gameObject);
      }

      // Reset rotation
      transform.rotation = Quaternion.identity;

      _mediaDisplays.Clear();
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
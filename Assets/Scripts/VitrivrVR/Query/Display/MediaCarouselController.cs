using System.Collections.Generic;
using System.Linq;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using VitrivrVR.Config;
using VitrivrVR.Media.Display;

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
    public float itemAngle = 15f; // Angle between media items
    public float angleNoise = 10f;
    public string scrollAxis = "Horizontal";
    public float scrollSpeed = 30f;

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private readonly Queue<ScoredSegment> _instantiationQueue = new Queue<ScoredSegment>();

    private void Update()
    {
      // Rotate carousel
      var scroll = UnityEngine.Input.GetAxisRaw(scrollAxis);
      var transform1 = transform;
      transform1.Rotate(Vector3.up, Time.deltaTime * scrollSpeed * scroll);

      if (_instantiationQueue.Count > 0)
      {
        CreateResultObject(_instantiationQueue.Dequeue());
      }
    }

    private void CreateResultObject(ScoredSegment result)
    {
      // Determine position
      var position = new Vector3(0, 0, innerRadius + 1 - (float) result.score);
      var column = _mediaDisplays.Count % rows - (rows - 1) / 2;
      var row = _mediaDisplays.Count / rows;
      position = Quaternion.Euler(column * itemAngle + Random.Range(-angleNoise, angleNoise),
        row * itemAngle + Random.Range(-angleNoise, angleNoise), 0) * position;
      var targetPosition = transform.position;
      position += targetPosition; // Move result display focus a little bit higher
      // Rotate media display to face center
      var rotation = Quaternion.LookRotation(position - targetPosition, Vector3.up);

      var itemDisplay = Instantiate(mediaItemDisplay, position, rotation, transform);

      _mediaDisplays.Add((itemDisplay, (float) result.score));

      // Only begin initialization after determining position so that results can begin positioning
      itemDisplay.Initialize(result);
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

    protected override void Initialize()
    {
      var fusionResults = queryData.GetMeanFusionResults();
      foreach (var segment in fusionResults.Take(ConfigManager.Config.maxDisplay))
      {
        _instantiationQueue.Enqueue(segment);
      }
    }

    private void OnDestroy()
    {
      ClearResults();
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Media.Controller;

namespace VitrivrVR.Query.Display
{
  public class MapLocationDisplay : MonoBehaviour
  {
    public int displayLocations = 100;
    public ThumbnailController locationPrefab;

    private Map.Map _map;
    private List<(ScoredSegment segment, Vector2 location)> _locations = new();
    private List<ThumbnailController> _locationObjects = new();

    private Vector2 _lastLocation = Vector2.zero;
    private float _lastZoom;

    private Dictionary<string, ThumbnailController> _visibleLocations = new();

    private void Start()
    {
      _map = GetComponent<Map.Map>();

      for (var i = 0; i < displayLocations; i++)
      {
        var locationObject = Instantiate(locationPrefab, transform);
        _locationObjects.Add(locationObject);
        locationObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        locationObject.scaleFactor = 10;
        locationObject.gameObject.SetActive(false);
      }
    }

    public void SetLocations(List<(ScoredSegment, Vector2)> locations)
    {
      _locations = locations;
    }

    private async void FixedUpdate()
    {
      if (_locations.Count == 0)
      {
        return;
      }

      var currentLocation = _map.Coordinates;
      var currentZoom = _map.Zoom;

      if (_lastLocation == currentLocation && Mathf.Approximately(_lastZoom, currentZoom))
      {
        return;
      }

      var positions = GetVisibleLocations();

      // Disable non-visible locations
      _visibleLocations.Keys.Except(positions.Select(pair => pair.segment.segment.Id))
        .ToList()
        .ForEach(key =>
        {
          _visibleLocations[key].gameObject.SetActive(false);
          _visibleLocations.Remove(key);
        });

      var availableObjects = _locationObjects.Where(obj => !obj.gameObject.activeSelf).ToList();

      const float liftFactor = 0.001f;
      var up = _map.transform.up;

      // Update / enable visible locations
      // Try to prioritize results with high scores
      positions.Sort((a, b) => b.segment.score.CompareTo(a.segment.score));
      foreach (var (position, segment) in positions)
      {
        if (_visibleLocations.TryGetValue(segment.segment.Id, out var location))
        {
          location.transform.position = position + up * (liftFactor * (1 + (float)segment.score));
        }
        else if (availableObjects.Count > 0)
        {
          var locationObject = availableObjects[0];
          availableObjects.RemoveAt(0);
          locationObject.transform.position = position + up * (liftFactor * (1 + (float)segment.score));
          locationObject.gameObject.SetActive(true);
          locationObject.url = await segment.segment.GetThumbnailUrl();
          locationObject.StartDownload();
          _visibleLocations[segment.segment.Id] = locationObject;
        }
      }

      _lastLocation = currentLocation;
      _lastZoom = currentZoom;
    }

    private List<(Vector3 position, ScoredSegment segment)> GetVisibleLocations()
    {
      var mapPosition = _map.transform.position;
      var maxDistance = _map.maxDistance * _map.maxDistance;
      return _locations.Select(pair =>
        {
          var position = _map.CoordinatesToPosition(pair.location);
          var sqrMagnitude = (position - mapPosition).sqrMagnitude;
          return (position, sqrMagnitude, pair.segment);
        })
        .Where(triple => triple.sqrMagnitude < maxDistance)
        .Select(triple => (triple.position, triple.segment)).ToList();
    }
  }
}
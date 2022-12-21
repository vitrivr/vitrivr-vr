using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Logging;
using VitrivrVR.Media.Display;
using VitrivrVR.Notification;
using static VitrivrVR.Logging.Interaction;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Displays queries as if on the surface of a cylinder in a grid.
  /// Object version
  /// </summary>
  public class CylinderObjectQueryDisplay : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public int rows = 4;
    public float rotationSpeed = 90;
    public float distance = 1;
    public float resultSize = 0.2f;
    public float padding = 0.02f;

    public InputAction rotationAction;

    public int maxSegmentsPerObject = 3;
    public float segmentDistance = 0.2f;

    public override int NumberOfResults => _nResults;

    private readonly Queue<List<ScoredSegment>> _instantiationQueue = new();

    private List<List<ScoredSegment>> _results;

    private int _enqueued;

    private int _instantiated;

    private readonly List<List<MediaItemDisplay>> _mediaObjectSegmentDisplays = new();

    private int _nResults;
    private float _columnAngle;
    private int _maxColumns;

    private float _currentRotation;
    private int _currentStart;
    private int _currentEnd;

    private void Awake()
    {
      var angle = 2 * Mathf.Atan((resultSize + padding) / (2 * distance));
      _maxColumns = Mathf.FloorToInt(2 * Mathf.PI / angle);

      if (_maxColumns * rows > ConfigManager.Config.maxDisplay)
      {
        _maxColumns = ConfigManager.Config.maxDisplay / rows;
      }

      _columnAngle = Mathf.Rad2Deg * (2 * Mathf.PI / _maxColumns);
    }

    private void OnEnable()
    {
      rotationAction.Enable();
    }

    private void OnDisable()
    {
      rotationAction.Disable();
    }

    private void Update()
    {
      Rotate(Time.deltaTime * rotationSpeed * rotationAction.ReadValue<Vector2>().x);

      if (_instantiationQueue.Count > 0)
      {
        CreateResultObject(_instantiationQueue.Dequeue());
      }
    }

    protected override async void Initialize()
    {
      var fusionResults = QueryData.GetMeanFusionResults();
      if (fusionResults == null)
      {
        NotificationController.Notify("No results returned from query!");
        fusionResults = new List<ScoredSegment>();
      }

      var resultsWithObjectIds =
        await Task.WhenAll(fusionResults.Select(async segment => (segment, await segment.segment.GetObjectId())));

      _results = (
        from tuple in resultsWithObjectIds
        group tuple.segment by tuple.Item2
        into resultGroup
        orderby resultGroup.First().score descending
        select resultGroup.ToList()
      ).ToList();

      _nResults = _results.Count;
      foreach (var objectSegments in _results.Take(_maxColumns * 3 / 4 * rows))
      {
        _instantiationQueue.Enqueue(objectSegments);
        _enqueued++;
      }

      LoggingController.LogQueryResults("object", fusionResults, QueryData);
    }

    /// <summary>
    /// Rotates the display and ensures that only displays that are supposed to be within the viewing window are
    /// visible. Adds uninitialized segments to the instantiation queue as needed.
    /// </summary>
    private void Rotate(float degrees)
    {
      transform.Rotate(degrees * Vector3.up);
      _currentRotation -= degrees;
      // Subtract 90 from current rotation to have results replaced on the side and not in front of the user
      var rawColumnIndex = Mathf.FloorToInt((_currentRotation - 90) / _columnAngle);

      // Check instantiations
      var enabledEnd = Mathf.Min((rawColumnIndex + _maxColumns) * rows, _nResults);
      if (enabledEnd > _mediaObjectSegmentDisplays.Count && _enqueued == _instantiated)
      {
        var index = _instantiated;
        if (index < _results.Count)
        {
          _instantiationQueue.Enqueue(_results[index]);
          _enqueued++;
        }
      }

      // Check enabled
      var enabledStart = Math.Max(rawColumnIndex * rows, 0);
      if (enabledStart != _currentStart || enabledEnd != _currentEnd)
      {
        var start = Mathf.Min(enabledStart, _currentStart);
        var end = Mathf.Min(Mathf.Max(enabledEnd, _currentEnd), _mediaObjectSegmentDisplays.Count);
        for (var i = start; i < end; i++)
        {
          var active = enabledStart <= i && i < enabledEnd;
          _mediaObjectSegmentDisplays[i].ForEach(display => display.gameObject.SetActive(active));
        }

        _currentStart = enabledStart;
        _currentEnd = enabledEnd;

        LoggingController.LogInteraction("rankedList", $"browse {Mathf.Sign(degrees)}", Browsing);
      }
    }

    private void CreateResultObject(IEnumerable<ScoredSegment> objectResult)
    {
      var index = _mediaObjectSegmentDisplays.Count;

      _mediaObjectSegmentDisplays.Add(objectResult
        .Take(maxSegmentsPerObject)
        .Select((scoredSegment, i) =>
        {
          var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, Quaternion.identity, transform);
          var (position, rotation) = GetResultLocalPosRot(index, i * segmentDistance);

          var t = itemDisplay.transform;
          t.localPosition = position;
          t.localRotation = rotation;
          // Adjust size
          t.localScale *= resultSize;

          itemDisplay.Initialize(scoredSegment);

          itemDisplay.gameObject.SetActive(_currentStart <= index && index < _currentEnd);

          return itemDisplay;
        }).ToList());

      _instantiated++;
    }

    /// <summary>
    /// Calculates and returns the local position and rotation of a result display based on its index.
    /// The distanceDelta parameter can be used to specify additional distance from the display cylinder.
    /// </summary>
    private (Vector3 position, Quaternion rotation) GetResultLocalPosRot(int index, float distanceDelta = 0)
    {
      var row = index % rows;
      var column = index / rows;
      var multiplier = resultSize + padding;
      var position = new Vector3(0, multiplier * row, distance + distanceDelta);
      var rotation = Quaternion.Euler(0, column * _columnAngle, 0);
      position = rotation * position;

      return (position, rotation);
    }
  }
}
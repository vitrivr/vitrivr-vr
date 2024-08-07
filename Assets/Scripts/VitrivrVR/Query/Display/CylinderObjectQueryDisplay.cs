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
    public Transform mediaObjectItemDisplay;
    public int rows = 4;
    public float rotationSpeed = 90;
    public float distance = 1;
    public float resultSize = 0.2f;
    public float padding = 0.02f;

    public InputAction rotationAction;

    public int maxSegmentsPerObject = 3;

    public override int NumberOfResults => _nResults;

    private const float SegmentDistance = .3f;

    private readonly Queue<List<ScoredSegment>> _instantiationQueue = new();

    private List<List<ScoredSegment>> _results;

    private int _enqueued;

    private int _instantiated;

    private readonly List<Transform> _mediaObjectSegmentDisplays = new();

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

      if (ConfigManager.Config.reduceMotion)
      {
        rotationAction.started += context =>
        {
          var value = context.ReadValue<Vector2>().x;
          if (value != 0)
          {
            Rotate(ConfigManager.Config.reduceMotionAngle * Mathf.Sign(value));
          }
        };
      }
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
      if (ConfigManager.Config.reduceMotion)
      {
        Rotate(0); // To ensure update
      }
      else
      {
        Rotate(Time.deltaTime * rotationSpeed * rotationAction.ReadValue<Vector2>().x);
      }

      if (_instantiationQueue.Count > 0)
      {
        CreateResultObject(_instantiationQueue.Dequeue());
      }
    }

    protected override async void Initialize()
    {
      var fusionResults = ScoreFusionUtil.FuseScores(QueryData);
      if (fusionResults == null)
      {
        NotificationController.Notify("No results returned from query!");
        fusionResults = new List<ScoredSegment>();
      }

      var resultsWithObjectIds = await Task.WhenAll(fusionResults
        .Where(segment => segment.segment.Initialized) // Prevents erroneous segments from preventing initialization
        .Select(async segment => (segment, await segment.segment.GetObjectId()))
      );

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
      if (enabledStart == _currentStart && enabledEnd == _currentEnd) return;

      var start = Mathf.Min(enabledStart, _currentStart);
      var end = Mathf.Min(Mathf.Max(enabledEnd, _currentEnd), _mediaObjectSegmentDisplays.Count);
      for (var i = start; i < end; i++)
      {
        var active = enabledStart <= i && i < enabledEnd;
        _mediaObjectSegmentDisplays[i].gameObject.SetActive(active);
      }

      _currentStart = enabledStart;
      _currentEnd = enabledEnd;

      LoggingController.LogInteraction("rankedList", $"browse {Mathf.Sign(degrees)}", Browsing);
    }

    private void CreateResultObject(IEnumerable<ScoredSegment> objectResult)
    {
      var index = _mediaObjectSegmentDisplays.Count;

      // Instantiate drawer like display holder
      var mediaObjectDisplay = Instantiate(mediaObjectItemDisplay, Vector3.zero, Quaternion.identity, transform);
      var (position, rotation) = GetResultLocalPosRot(index);

      // Set position and rotation
      mediaObjectDisplay.localPosition = position;
      mediaObjectDisplay.localRotation = rotation;
      // Adjust size
      mediaObjectDisplay.localScale *= resultSize;

      // Get the grab enabled display parent to instantiate segment displays into
      var displayParent = mediaObjectDisplay.GetChild(0);

      _mediaObjectSegmentDisplays.Add(mediaObjectDisplay);


      // Ensure only the set maximum of segments is displayed
      var enumeratedSegments = objectResult
        .Take(maxSegmentsPerObject)
        .Select((scoredSegment, i) => (scoredSegment, i))
        .ToList();

      // Create segment displays
      enumeratedSegments.ForEach(pair =>
      {
        var (scoredSegment, i) = pair;
        var itemDisplay = Instantiate(mediaItemDisplay, displayParent);

        // Adjust local position based on index and set local rotation to identity
        var t = itemDisplay.transform;
        t.localPosition = Vector3.forward * (i * SegmentDistance);
        t.localRotation = Quaternion.identity;

        itemDisplay.Initialize(scoredSegment);
      });

      // Set grab enabled bounding box size
      if (!displayParent.TryGetComponent<BoxCollider>(out var boxCollider))
        throw new Exception("Could not get BoxCollider!");
      var size = boxCollider.size;
      var center = boxCollider.center;

      size.z = enumeratedSegments.Count * SegmentDistance;
      center.z = (size.z - SegmentDistance) / 2;

      boxCollider.size = size;
      boxCollider.center = center;

      // Set disabled if outside of active range
      mediaObjectDisplay.gameObject.SetActive(_currentStart <= index && index < _currentEnd);

      _instantiated++;
    }

    /// <summary>
    /// Calculates and returns the local position and rotation of a result display based on its index.
    /// </summary>
    private (Vector3 position, Quaternion rotation) GetResultLocalPosRot(int index)
    {
      var row = index % rows;
      var column = index / rows;
      var multiplier = resultSize + padding;
      var position = new Vector3(0, multiplier * row, distance);
      var rotation = Quaternion.Euler(0, column * _columnAngle, 0);
      position = rotation * position;

      return (position, rotation);
    }
  }
}
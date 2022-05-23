using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VitrivrVR.Config;
using VitrivrVR.Media.Display;
using VitrivrVR.Notification;
using VitrivrVR.Submission;

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

    private readonly List<MediaItemDisplay> _mediaDisplays = new List<MediaItemDisplay>();

    private readonly Queue<ScoredSegment> _instantiationQueue = new Queue<ScoredSegment>();

    private List<ScoredSegment> _results;

    private int _enqueued;

    /// <summary>
    /// Dictionary containing for each media object in the results the index of the list of corresponding media item
    /// displays in _mediaObjectSegmentDisplays.
    /// </summary>
    private readonly Dictionary<string, int> _objectMap = new Dictionary<string, int>();

    private readonly List<List<MediaItemDisplay>> _mediaObjectSegmentDisplays = new List<List<MediaItemDisplay>>();

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

    private async void Update()
    {
      Rotate(Time.deltaTime * rotationSpeed * rotationAction.ReadValue<Vector2>().x);

      if (_instantiationQueue.Count > 0)
      {
        await CreateResultObject(_instantiationQueue.Dequeue());
      }
    }

    protected override void Initialize()
    {
      var fusionResults = queryData.GetMeanFusionResults();
      _results = fusionResults;
      if (_results == null)
      {
        NotificationController.Notify("No results returned from query!");
        _results = new List<ScoredSegment>();
      }

      _nResults = _results.Count;
      foreach (var segment in _results.Take(_maxColumns * 3 / 4 * rows))
      {
        _instantiationQueue.Enqueue(segment);
        _enqueued++;
      }

      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogResults("object", _results, queryData.Query);
      }
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
      if (enabledEnd > _mediaObjectSegmentDisplays.Count && _enqueued == _mediaDisplays.Count)
      {
        var index = _mediaDisplays.Count;
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
      }
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      var objectId = await result.segment.GetObjectId();

      // Only instantiate if max segments for this object have not been reached already
      if (_objectMap.ContainsKey(objectId) &&
          _mediaObjectSegmentDisplays[_objectMap[objectId]].Count >= maxSegmentsPerObject)
      {
        _mediaDisplays.Add(null);
        return;
      }

      var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, Quaternion.identity, transform);

      // Add to media object list
      if (_objectMap.ContainsKey(objectId))
      {
        _mediaObjectSegmentDisplays[_objectMap[objectId]].Add(itemDisplay);
      }
      else
      {
        _objectMap[objectId] = _mediaObjectSegmentDisplays.Count;
        _mediaObjectSegmentDisplays.Add(new List<MediaItemDisplay> {itemDisplay});
      }

      var index = _objectMap[objectId];
      var (position, rotation) =
        GetResultLocalPosRot(index, (_mediaObjectSegmentDisplays[index].Count - 1) * segmentDistance);

      var transform2 = itemDisplay.transform;
      transform2.localPosition = position;
      transform2.localRotation = rotation;
      // Adjust size
      transform2.localScale *= resultSize;

      _mediaDisplays.Add(itemDisplay);
      itemDisplay.Initialize(result);

      itemDisplay.gameObject.SetActive(_currentStart <= index && index < _currentEnd);
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
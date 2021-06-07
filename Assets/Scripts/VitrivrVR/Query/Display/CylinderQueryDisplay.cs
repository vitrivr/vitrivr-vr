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

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Displays queries as if on the surface of a cylinder in a grid.
  /// </summary>
  public class CylinderQueryDisplay : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public int rows = 4;
    public float rotationSpeed;
    public float distance;
    public float resultSize;
    public float padding = 0.2f;

    public InputAction rotationAction;

    public override int NumberOfResults => _nResults;

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private readonly Queue<ScoredSegment> _instantiationQueue = new Queue<ScoredSegment>();

    private List<ScoredSegment> _results;

    /// <summary>
    /// Dictionary containing for each media object in the results the index in the media object display mode and the
    /// number of segments belonging to it found in the results so far.
    /// </summary>
    private Dictionary<string, (int firstIndex, int segments)> _mediaObjectIndexes =
      new Dictionary<string, (int firstIndex, int segments)>();

    private int _nResults;
    private float _columnAngle;
    private int _maxColumns;

    private float _currentRotation;
    private int _currentStart;
    private int _currentEnd;

    private DisplayMode _currentDisplayMode = DisplayMode.MediaSegmentDisplay;

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

    public override void Initialize(QueryResponse queryData)
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
      }
    }

    public override async void SwitchDisplayMode(DisplayMode mode)
    {
      if (mode == _currentDisplayMode)
      {
        return;
      }

      switch (mode)
      {
        case DisplayMode.MediaSegmentDisplay:
          for (var i = 0; i < _mediaDisplays.Count; i++)
          {
            var (position, rotation) = GetResultLocalPosRot(i);
            var displayTransform = _mediaDisplays[i].display.transform;
            displayTransform.localPosition = position;
            displayTransform.localRotation = rotation;
          }

          break;
        case DisplayMode.MediaObjectDisplay:
          for (var i = 0; i < _mediaDisplays.Count; i++)
          {
            var objectId = await _results[i].segment.GetObjectId();
            int index;
            var count = 0;
            if (_mediaObjectIndexes.ContainsKey(objectId))
            {
              (index, count) = _mediaObjectIndexes[objectId];
            }
            else
            {
              index = _mediaObjectIndexes.Count;
            }

            _mediaObjectIndexes[objectId] = (index, count + 1);

            var (position, rotation) = GetResultLocalPosRot(index, count * padding);
            var displayTransform = _mediaDisplays[i].display.transform;
            displayTransform.localPosition = position;
            displayTransform.localRotation = rotation;
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
      }

      _currentDisplayMode = mode;
    }

    private void Rotate(float degrees)
    {
      transform.Rotate(degrees * Vector3.up);
      _currentRotation -= degrees;
      // Subtract 90 from current rotation to have results replaced on the side and not in front of the user
      var rawColumnIndex = Mathf.FloorToInt((_currentRotation - 90) / _columnAngle);

      // Check instantiations
      var enabledEnd = Mathf.Min((rawColumnIndex + _maxColumns) * rows, _nResults);
      if (enabledEnd > _mediaDisplays.Count + _instantiationQueue.Count)
      {
        var start = _mediaDisplays.Count + _instantiationQueue.Count;
        foreach (var segment in _results.GetRange(start, enabledEnd - start))
        {
          _instantiationQueue.Enqueue(segment);
        }
      }

      // Check enabled
      var enabledStart = Math.Max(rawColumnIndex * rows, 0);
      if (enabledStart != _currentStart || enabledEnd != _currentEnd)
      {
        var start = Mathf.Min(enabledStart, _currentStart);
        var end = Mathf.Min(Mathf.Max(enabledEnd, _currentEnd), _mediaDisplays.Count);
        for (var i = start; i < end; i++)
        {
          _mediaDisplays[i].display.gameObject.SetActive(enabledStart <= i && i < enabledEnd);
        }

        _currentStart = enabledStart;
        _currentEnd = enabledEnd;
      }
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      // Determine position
      var index = _mediaDisplays.Count;

      var (position, rotation) = GetResultLocalPosRot(index);

      var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, Quaternion.identity, transform);
      var transform2 = itemDisplay.transform;
      transform2.localPosition = position;
      transform2.localRotation = rotation;
      // Adjust size
      transform2.localScale *= resultSize;


      _mediaDisplays.Add((itemDisplay, (float) result.score));

      await itemDisplay.Initialize(result);

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
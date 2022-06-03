using System;
using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using UnityEngine.InputSystem;
using VitrivrVR.Config;
using VitrivrVR.Media.Display;
using VitrivrVR.Notification;
using VitrivrVR.Submission;

namespace VitrivrVR.Query.Display
{
  public class CylinderTemporalQueryDisplay : TemporalQueryDisplay
  {
    public TemporalMediaItemDisplay temporalMediaItemDisplay;
    public int rows = 4;
    public float rotationSpeed = 90;
    public float distance = 1;
    public float resultSize = 0.2f;
    public float padding = 0.02f;

    public InputAction rotationAction;

    public override int NumberOfResults => _nResults;

    private readonly List<TemporalMediaItemDisplay> _mediaDisplays = new();

    private readonly Queue<TemporalObject> _instantiationQueue = new();

    private List<TemporalObject> _results;

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

    protected override void Initialize()
    {
      _results = temporalQueryData.Results.Content;
      
      if (_results.Count == 0)
      {
        NotificationController.Notify("No results returned from query!");
      }

      _nResults = _results.Count;
      foreach (var temporalObject in _results.Take(_maxColumns * 3 / 4 * rows))
      {
        _instantiationQueue.Enqueue(temporalObject);
      }

      if (ConfigManager.Config.dresEnabled)
      {
        DresClientManager.LogResults("temporal", _results, temporalQueryData.Query);
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
          var active = enabledStart <= i && i < enabledEnd;
          _mediaDisplays[i].gameObject.SetActive(active);
        }

        _currentStart = enabledStart;
        _currentEnd = enabledEnd;

        DresClientManager.LogInteraction("rankedList", $"browse {Mathf.Sign(degrees)}");
      }
    }

    private void CreateResultObject(TemporalObject temporalObject)
    {
      // Determine position
      var index = _mediaDisplays.Count;
      var (position, rotation) = GetResultLocalPosRot(index);

      var itemDisplay = Instantiate(temporalMediaItemDisplay, Vector3.zero, Quaternion.identity, transform);

      var transform2 = itemDisplay.transform;
      transform2.localPosition = position;
      transform2.localRotation = rotation;
      // Adjust size
      transform2.localScale *= resultSize;

      // Add to media displays list
      _mediaDisplays.Add(itemDisplay);

      itemDisplay.Initialize(temporalObject);

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
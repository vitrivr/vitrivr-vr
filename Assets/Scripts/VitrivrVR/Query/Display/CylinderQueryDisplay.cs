using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using VitrivrVR.Config;
using VitrivrVR.Media;
using VitrivrVR.Notification;

namespace VitrivrVR.Query.Display
{
  public class CylinderQueryDisplay : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public int rows = 4;
    public float rotationSpeed;
    public float distance;
    public float resultSize;
    public float padding = 0.2f;

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private readonly Queue<ScoredSegment> _instantiationQueue = new Queue<ScoredSegment>();

    private List<ScoredSegment> _results;
    private int _nResults;
    private float _columnAngle;
    private int _maxColumns;
    private float _currentRotation;

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

    private async void Update()
    {
      Rotate(Time.deltaTime * rotationSpeed * UnityEngine.Input.GetAxisRaw("Horizontal"));
      
      // Start includes items in the instantiation queue, since these will be instantiated shortly
      // var start = _mediaDisplays.Count + _instantiationQueue.Count;
      // var columns = start / rows;
      //
      // if (transform.position.x - rows * loadBuffer < -(1 + padding) * columns)
      // {
      //   var end = Mathf.Min((columns + 1) * rows, _nResults);
      //   if (start < _nResults)
      //   {
      //     foreach (var segment in _results.GetRange(start, end - start))
      //     {
      //       _instantiationQueue.Enqueue(segment);
      //     }
      //   }
      // }

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

    private void Rotate(float degrees)
    {
      _currentRotation -= degrees;
      transform.Rotate(degrees * Vector3.up);
      
      // Check instantiations
      var enabledEnd = Mathf.Min(Mathf.FloorToInt((_currentRotation + 360) / _columnAngle) * rows, _nResults);
      if (enabledEnd > _mediaDisplays.Count + _instantiationQueue.Count)
      {
        var start = _mediaDisplays.Count + _instantiationQueue.Count;
        foreach (var segment in _results.GetRange(start, enabledEnd - start))
        {
          _instantiationQueue.Enqueue(segment);
        }
      }
      
      // Check enabled
      // TODO: Find better way to check only relevant displays rather than iterating over all displays every frame
      var enabledStart = Math.Max(Mathf.FloorToInt(_currentRotation / _columnAngle) * rows, 0);
      for (var i = 0; i < _mediaDisplays.Count; i++)
      {
        _mediaDisplays[i].display.gameObject.SetActive(enabledStart <= i && i < enabledEnd);
      }
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      // Determine position
      var row = _mediaDisplays.Count % rows;
      var column = _mediaDisplays.Count / rows;
      var multiplier = resultSize + padding;
      var position = new Vector3(0, multiplier * row, distance);
      var rotation = Quaternion.Euler(0, column * _columnAngle, 0);
      position = rotation * position;

      var itemDisplay = Instantiate(mediaItemDisplay, Vector3.zero, Quaternion.identity, transform);
      var transform2 = itemDisplay.transform;
      transform2.localPosition = position;
      transform2.localRotation = rotation;
      // Adjust size
      transform2.localScale *= resultSize;

      _mediaDisplays.Add((itemDisplay, (float) result.score));

      await itemDisplay.Initialize(result);
    }
  }
}
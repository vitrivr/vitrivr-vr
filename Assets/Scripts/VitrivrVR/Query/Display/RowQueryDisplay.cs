using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VitrivrVR.Config;
using VitrivrVR.Media;

namespace VitrivrVR.Query.Display
{
  public class RowQueryDisplay : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public int rows = 3;
    public float scrollSpeed;
    public float distance;
    public float resultSize;
    public float padding = 0.2f;

    /// <summary>
    /// Number of columns of results to display at a minimum.
    /// </summary>
    public int loadBuffer = 10;

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private readonly Queue<ScoredSegment> _instantiationQueue = new Queue<ScoredSegment>();

    private List<ScoredSegment> _results;
    private int _nResults;

    private float _horizontalScroll;

    void OnLeftHandAxis(InputValue value)
    {
      _horizontalScroll = value.Get<Vector2>().x;
    }

    private async void Update()
    {
      transform.Translate(Time.deltaTime * scrollSpeed * _horizontalScroll * Vector3.left);
      // Start includes items in the instantiation queue, since these will be instantiated shortly
      var start = _mediaDisplays.Count + _instantiationQueue.Count;
      var columns = start / rows;

      if (transform.position.x - rows * loadBuffer < -(1 + padding) * columns)
      {
        var end = Mathf.Min((columns + 1) * rows, _nResults);
        if (start < _nResults)
        {
          foreach (var segment in _results.GetRange(start, end - start))
          {
            _instantiationQueue.Enqueue(segment);
          }
        }
      }
      
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
        Debug.Log("No results returned from query!");
        // TODO: Handle no results case fully (user notification etc.)
        _results = new List<ScoredSegment>();
      }
      _nResults = _results.Count;
      foreach (var segment in _results.Take(ConfigManager.Config.maxDisplay))
      {
        _instantiationQueue.Enqueue(segment);
      }
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      // Determine position
      var row = _mediaDisplays.Count % rows;
      var column = _mediaDisplays.Count / rows;
      var multiplier = resultSize + padding;
      var position = new Vector3(multiplier * column, multiplier * row, distance);
      var transform1 = transform;
      var targetPosition = transform1.position;
      position += targetPosition;

      var itemDisplay = Instantiate(mediaItemDisplay, position, Quaternion.identity, transform1);
      // Adjust size
      itemDisplay.transform.localScale *= resultSize;

      _mediaDisplays.Add((itemDisplay, (float) result.score));

      await itemDisplay.Initialize(result);
    }
  }
}
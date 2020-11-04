using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
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
    public float padding = 0.2f;
    /// <summary>
    /// Number of columns of results to display at a minimum.
    /// </summary>
    public int loadBuffer = 10;

    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private List<ScoredSegment> _results;
    private int _nResults;

    private async void Update()
    {
      var scroll = UnityEngine.Input.GetAxisRaw("Horizontal");
      transform.Translate(Time.deltaTime * scrollSpeed * scroll * Vector3.left);
      var columns = _mediaDisplays.Count / rows;

      if (transform.position.x - rows * loadBuffer < -(1 + padding) * columns)
      {
        var start = _mediaDisplays.Count;
        var end = Mathf.Min((columns + 1) * rows, _nResults);
        if (start < _nResults)
        {
          var tasks = _results.GetRange(start, end - start).Select(CreateResultObject);
          await Task.WhenAll(tasks);
        }
      }
    }

    public override async void Initialize(QueryResponse queryData)
    {
      var fusionResults = queryData.GetMeanFusionResults();
      _results = fusionResults;
      _nResults = _results.Count;
      var tasks = fusionResults
        .Take(ConfigManager.Config.maxDisplay)
        .Select(CreateResultObject);
      await Task.WhenAll(tasks);
    }

    private async Task CreateResultObject(ScoredSegment result)
    {
      // Determine position
      var row = _mediaDisplays.Count % rows;
      var column = _mediaDisplays.Count / rows;
      var multiplier = 1 + padding;
      var position = new Vector3(multiplier * column, multiplier * row, distance);
      var transform1 = transform;
      var targetPosition = transform1.position;
      position += targetPosition;

      var itemDisplay = Instantiate(mediaItemDisplay, position, Quaternion.identity, transform1);

      _mediaDisplays.Add((itemDisplay, (float) result.score));

      await itemDisplay.Initialize(result);
    }
  }
}
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
    
    private readonly List<(MediaItemDisplay display, float score)> _mediaDisplays =
      new List<(MediaItemDisplay, float)>();

    private void Update()
    {
      var scroll = UnityEngine.Input.GetAxisRaw("Horizontal");
      transform.Translate(Time.deltaTime * scrollSpeed * scroll * Vector3.left);
    }

    public override async void Initialize(QueryResponse queryData)
    {
      var fusionResults = queryData.GetMeanFusionResults();
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
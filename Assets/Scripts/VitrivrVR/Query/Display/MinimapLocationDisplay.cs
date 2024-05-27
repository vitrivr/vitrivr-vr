using System.Linq;
using UnityEngine;

namespace VitrivrVR.Query.Display
{
  public class MinimapLocationDisplay : MonoBehaviour
  {
    public ParticleSystem system;

    private void Start()
    {
      QueryController.Instance.queryFocusEvent.AddListener(OnQueryFocus);
    }

    private async void OnQueryFocus(int oldIndex, int newIndex)
    {
      system.Clear();
      if (newIndex == -1)
      {
        return;
      }

      // Get query results
      var results = ScoreFusionUtil.FuseScores(QueryController.Instance.queries[newIndex].QueryData);
      var client = QueryController.Instance.queries[newIndex].QueryClient;
      var locationsResult = await client.LoadVectors(results.Select(segment => segment.segment.Id).ToList(),
        "spatialdistance");

      const float radius = 0.505f;
      const float size = 0.01f;
      var locations = locationsResult.Points.Select(point =>
        {
          var x = (90 - point.Vector[0]) / 180 * Mathf.PI;
          var y = -point.Vector[1] / 180 * Mathf.PI;

          return new Vector3(
            radius * Mathf.Sin(x) * Mathf.Cos(y),
            radius * Mathf.Cos(x), -radius * Mathf.Sin(x) * Mathf.Sin(y));
        })
        .ToList();

      var mainConfig = system.main;
      mainConfig.maxParticles = locations.Count;

      foreach (var emitParams in locations.Select(item => new ParticleSystem.EmitParams
               {
                 position = item,
                 velocity = Vector3.zero,
                 startLifetime = float.PositiveInfinity,
                 startSize = size,
                 startColor = Color.red
               }))
      {
        system.Emit(emitParams, 1);
      }
    }
  }
}
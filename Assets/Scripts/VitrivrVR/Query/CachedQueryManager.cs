using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Config;

namespace VitrivrVR.Query
{
  internal record CachedClient(CineastClient Client, long LastQuery)
  {
    public CineastClient Client { get; } = Client;
    public long LastQuery { get; set; } = LastQuery;
  }

  public class CachedQueryManager : MonoBehaviour
  {
    public float updateInterval = 1;

    private List<CachedClient> _clients;

    private float _updateTime;

    private void Start()
    {
      _clients = QueryController.Instance.CineastClients.Select(client => new CachedClient(client, 0L)).ToList();
    }

    private async void Update()
    {
      _updateTime += Time.deltaTime;
      if (_updateTime < updateInterval) return;

      _updateTime %= updateInterval;

      foreach (var cachedClient in _clients)
      {
        var (client, lastQuery) = cachedClient;
        var cached = await client.ListCachedQueries();
        if (cached.Count == 0) continue;

        var mostRecent = cached.Aggregate((c1, c2) => c1.Timestamp > c2.Timestamp ? c1 : c2);
        if (mostRecent.Timestamp <= lastQuery) continue;

        cachedClient.LastQuery = mostRecent.Timestamp;

        var queryResults = await client.SegmentApi.FindSegmentByCachedQueryIdAsync(mostRecent.Id);
        var querySegments = ResultUtils.ToSegmentData(queryResults, client.MultimediaRegistry);
        var queryData = new QueryResponse(new SimilarityQuery(new List<QueryTerm>()), querySegments);
        await queryData.Prefetch(ConfigManager.Config.maxResults, client.MultimediaRegistry);

        QueryController.Instance.InstantiateQueryDisplay(queryData);
      }
    }
  }
}
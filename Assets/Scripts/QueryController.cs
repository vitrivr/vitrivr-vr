using System;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Api;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

public class QueryController : MonoBehaviour
{
    public TagInputController tagInputController;
    public int maxResults = 20;
    public MediaCarouselController mediaCarousel;

    private SegmentsApi _segmentsApi;
    private SegmentApi _segmentApi;

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;
    
    void Awake()
    {
        var apiConfig = CineastConfigManager.Instance.ApiConfiguration;

        _segmentsApi = new SegmentsApi(apiConfig);
        _segmentApi = new SegmentApi(apiConfig);
    }

    public async void RunQuery()
    {
        var localGuid = Guid.NewGuid();
        _localQueryGuid = localGuid;

        var tagItems = tagInputController.TagItems;

        if (tagItems.Count == 0)
        {
            Debug.Log("Cannot run query: No tags specified.");
            return;
        }

        mediaCarousel.ClearResults();

        var tags = tagItems.Select(tagItem => (tagItem.TagId, tagItem.TagName)).ToList();
        var query = QueryBuilder.BuildTagsSimilarityQuery(tags);
        var queryResults = await Task.Run(() => _segmentsApi.FindSegmentSimilar(query));

        if (_localQueryGuid != localGuid)
        {
            // A new query has been started while this one was still busy, discard results
            return;
        }

        IdList idList = new IdList();
        var topResults = queryResults.Results[0].Content.Take(maxResults).ToList();
        idList.Ids = topResults.Select(result => result.Key).ToList();
        var segmentQueryResults = await Task.Run(() => _segmentApi.FindSegmentByIdBatched(idList));

        if (_localQueryGuid != localGuid)
        {
            // A new query has been started while this one was still busy, discard results
            return;
        }

        mediaCarousel.CreateResults(segmentQueryResults.Content.Zip(topResults.Select(result => result.Value), (item, score) => (item, score)).ToList());
    }
}
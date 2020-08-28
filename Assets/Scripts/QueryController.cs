using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Config;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Api;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

public class QueryController : MonoBehaviour
{
    public TagInputController tagInputController;
    public ThumbnailController thumbnailTemplate;
    public int maxResults = 10;

    private SegmentsApi _segmentsApi;
    private SegmentApi _segmentApi;

    private List<GameObject> _thumbnails = new List<GameObject>();

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;

    private CineastConfig _cineastConfig;

    void Awake()
    {
        _cineastConfig = CineastConfigManager.Instance.Config;
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

        ClearResults();

        var tags = tagItems.Select(tagItem => (tagItem.TagId, tagItem.TagName)).ToList();
        var query = QueryBuilder.BuildTagsSimilarityQuery(tags);
        var queryResults = await Task.Run(() => _segmentsApi.FindSegmentSimilar(query));

        if (_localQueryGuid != localGuid)
        {
            // A new query has been started while this one was still busy, discard results
            return;
        }

        IdList idList = new IdList();
        idList.Ids = queryResults.Results[0].Content.Take(maxResults).Select(result => result.Key).ToList();
        var segmentQueryResults = await Task.Run(() => _segmentApi.FindSegmentByIdBatched(idList));

        if (_localQueryGuid != localGuid)
        {
            // A new query has been started while this one was still busy, discard results
            return;
        }

        foreach (var result in segmentQueryResults.Content)
        {
            var thumbnailController = Instantiate(thumbnailTemplate, new Vector3(_thumbnails.Count / 2, 1 -
                _thumbnails.Count % 2), Quaternion.identity);
            var thumbnailPath =
                PathResolver.ResolvePath(_cineastConfig.thumbnailPath, result.ObjectId, result.SegmentId);
            thumbnailController.URL = $"{_cineastConfig.mediaHost}{thumbnailPath}{_cineastConfig.thumbnailExtension}";
            _thumbnails.Add(thumbnailController.gameObject);
        }
    }

    public void ClearResults()
    {
        foreach (var thumbnail in _thumbnails)
        {
            Destroy(thumbnail);
        }

        _thumbnails.Clear();
    }
}
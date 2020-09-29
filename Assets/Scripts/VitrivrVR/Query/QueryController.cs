using System;
using System.Linq;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using VitrivrVR.Media;

namespace VitrivrVR.Query
{
  public class QueryController : MonoBehaviour
  {
    public TagInputController tagInputController;
    public int prefetch = 72;
    public MediaCarouselController mediaCarousel;

    /// <summary>
    /// Keeps track of the latest query to determine if results of a returning query are still relevant.
    /// </summary>
    private Guid _localQueryGuid;

    public async void RunQuery()
    {
      var localGuid = Guid.NewGuid();
      _localQueryGuid = localGuid;

      var tagItems = tagInputController.tagItems;

      if (tagItems.Count == 0)
      {
        Debug.Log("Cannot run query: No tags specified.");
        return;
      }

      mediaCarousel.ClearResults();
      
      // TODO: Interfaces for tag and text providers

      var tags = tagItems.Select(tagItem => (tagItem.TagId, tagItem.TagName)).ToList();
      var query = QueryBuilder.BuildTagsSimilarityQuery(tags);

      var queryData = await CineastWrapper.ExecuteQuery(query, 1000, prefetch);

      if (_localQueryGuid != localGuid)
      {
        // A new query has been started while this one was still busy, discard results
        return;
      }

      mediaCarousel.CreateResults(queryData);
    }
  }
}
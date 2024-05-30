using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  public class SimpleQueryTermManager : QueryTermManager
  {
    public GameObject booleanTermProviderPrefab;
    public GameObject mapTermProviderPrefab;

    private CanvasBooleanTermProvider _booleanTermProvider;
    private MapTermProvider _mapTermProvider;

    private string _textSearchText;
    private string _ocrSearchText;
    private string _ocrSearchCategory;
    private string _textSearchCategory;

    private void Start()
    {
      var config = Config.ConfigManager.Config;
      _ocrSearchCategory = config.textCategories.First().id;
      _textSearchCategory = config.textCategories[1].id;

      Debug.Log($"Using text category {_textSearchCategory} and OCR category {_ocrSearchCategory}");

      var booleanTermProvider =
        Instantiate(booleanTermProviderPrefab, new Vector3(0, 0.7f, 0.75f), Quaternion.identity);
      _booleanTermProvider = booleanTermProvider.GetComponentInChildren<CanvasBooleanTermProvider>();
      var mapTermProvider = Instantiate(mapTermProviderPrefab, new Vector3(0, 0.7f, 0.6f), Quaternion.identity);
      _mapTermProvider = mapTermProvider.GetComponentInChildren<MapTermProvider>();

      // Destroy or disable unnecessary canvases and clear buttons
      Destroy(booleanTermProvider.transform.Find("Canvas/Clear Button").gameObject);
      mapTermProvider.transform.Find("Map Term Canvas").gameObject.SetActive(false);
    }

    public void SetSearchText(string text)
    {
      _textSearchText = text;
    }
    
    public void SetOcrText(string text)
    {
      _ocrSearchText = text;
    }

    /// <summary>
    /// Generates the term list for the simple query term manager query.
    ///
    /// Query order (stages):
    /// 1. OCR term (most selective if provided)
    /// 2. Boolean terms (filtering before scoring)
    /// 3. Text term (most important score wise)
    /// 4. Map term (spatial filtering)
    /// </summary>
    public override List<List<List<QueryTerm>>> GetTerms()
    {
      var stages = new List<List<QueryTerm>>();
      if (!string.IsNullOrEmpty(_ocrSearchText))
      {
        stages.Add(new List<QueryTerm>
          {new(new List<string> {_ocrSearchCategory}, QueryTerm.TypeEnum.TEXT, _ocrSearchText)});
      }

      var booleanTerms = _booleanTermProvider.GetTerms();
      if (booleanTerms.Count > 0)
      {
        stages.Add(booleanTerms);
      }

      if (!string.IsNullOrEmpty(_textSearchText))
      {
        stages.Add(new List<QueryTerm>
          {new(new List<string> {_textSearchCategory}, QueryTerm.TypeEnum.TEXT, _textSearchText)});
      }


      SetMapHalfSimilarityDistance();
      var mapTerms = _mapTermProvider.GetTerms();
      if (mapTerms.Count > 0)
      {
        stages.Add(mapTerms);
      }

      return stages.Count > 0 ? new List<List<List<QueryTerm>>> {stages} : new List<List<List<QueryTerm>>>();
    }

    private void SetMapHalfSimilarityDistance()
    {
      var map = _mapTermProvider.map;
      var center = map.transform.position;
      var edge = center + map.transform.right * map.maxDistance;
      var start = map.PositionToCoordinates(center);
      var end = map.PositionToCoordinates(edge);

      var distance = DistanceBetweenCoordinates(start, end);

      _mapTermProvider.halfSimilarityDistance = distance;
    }

    private static float DistanceBetweenCoordinates(Vector2 point1, Vector2 point2)
    {
      const int earthRadius = 6371000;

      var diff = point2 - point1;

      var diffRadians = diff * Mathf.Deg2Rad;

      var sinLat = Mathf.Sin(diffRadians.x / 2);
      var sinLon = Mathf.Sin(diffRadians.y / 2);

      var radLat1 = point1.x * Mathf.Deg2Rad;
      var radLat2 = point2.x * Mathf.Deg2Rad;

      var a = sinLat * sinLat + sinLon * sinLon * Mathf.Cos(radLat1) * Mathf.Cos(radLat2);
      var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
      
      return earthRadius * c;
    }
  }
}
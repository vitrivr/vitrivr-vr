﻿using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using Vitrivr.UnityInterface.CineastApi.Utils;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// <see cref="QueryTermProvider"/> for spatial <see cref="QueryTerm"/>s through use of a <see cref="Map"/>.
  /// </summary>
  public class MapTermProvider : QueryTermProvider
  {
    public Map.Map map;
    public TMP_Text nameDisplayText;

    /// <summary>
    /// Half similarity distance to use for building query terms.
    /// Only used if > 0.
    /// </summary>
    public float halfSimilarityDistance = -1f;

    public override List<QueryTerm> GetTerms()
    {
      return map.gameObject.activeInHierarchy
        ? map.GetPinCoordinates()
          .Select(coordinates => halfSimilarityDistance > 0
            ? QueryTermBuilder.BuildLocationTerm(coordinates.x, coordinates.y, halfSimilarityDistance)
            : QueryTermBuilder.BuildLocationTerm(coordinates.x, coordinates.y))
          .ToList()
        : new List<QueryTerm>();
    }

    public override string GetTypeName()
    {
      return "Map";
    }

    public override void SetInstanceName(string displayName)
    {
      if (nameDisplayText != null)
      {
        nameDisplayText.text = displayName;
      }
    }

    public static void Clear()
    {
      // Get all UniversalPin objects at root level of the scene
      var pins = FindObjectsOfType<Map.UniversalPin>();
      foreach (var pin in pins)
      {
        pin.OnGrab();
        Destroy(pin.gameObject);
      }
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Utils;

namespace VitrivrVR.Query.Term.Pose
{
  public class PoseTermProvider : QueryTermProvider
  {
    public PoseProjectionController poseProjection;

    public override List<QueryTerm> GetTerms()
    {
      var skeletonValues = poseProjection.GetPoints();
      if (skeletonValues.Count == 0)
      {
        return new List<QueryTerm>();
      }

      var poseData = skeletonValues.Select(values =>
      {
        // Flip y coordinates because feature internally origin is top left and these coordinates are bottom left
        var coordinates = values.SelectMany(pair => new List<float> {pair.point.x, -pair.point.y}).ToList();
        var weights = values.Select(pair => pair.weight).ToList();

        return new PoseSkeleton(coordinates, weights);
      });

      var jsonData = "[" + string.Join(",", poseData.Select(JsonUtility.ToJson)) + "]";
      var base64data = Base64Converter.JsonToBase64(jsonData);

      return new List<QueryTerm> {new(QueryTerm.TypeEnum.SKELETON, base64data, new List<string> {"skeletonpose"})};
    }

    public override string GetTypeName()
    {
      return "Pose";
    }

    [Serializable]
    private record PoseSkeleton
    {
      public List<float> coordinates;
      public List<float> weights;

      public PoseSkeleton(List<float> coordinates, List<float> weights)
      {
        this.coordinates = coordinates;
        this.weights = weights;
      }
    }
  }
}
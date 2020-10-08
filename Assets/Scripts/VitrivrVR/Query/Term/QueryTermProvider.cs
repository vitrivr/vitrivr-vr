using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Abstract class for objects providing <see cref="QueryTerm"/>s.
  /// </summary>
  public abstract class QueryTermProvider : MonoBehaviour
  {
    public abstract List<QueryTerm> GetTerms();
  }
}
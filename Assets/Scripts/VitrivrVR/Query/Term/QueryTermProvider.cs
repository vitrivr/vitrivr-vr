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
    /// <summary>
    /// Returns a list of the <see cref="QueryTerm"/>s specified through this query term provider.
    ///
    /// In case this query term provider is disabled or currently does not specify any terms, expected behavior is to
    /// return an empty list.
    /// </summary>
    /// <returns>A list of <see cref="QueryTerm"/>s specified through this query term provider (may be empty).</returns>
    public abstract List<QueryTerm> GetTerms();

    /// <summary>
    /// Returns the descriptive name of the type of query term provider this is.
    /// </summary>
    /// <returns>The type of provider for display purposes.</returns>
    public abstract string GetTypeName();
  }
}
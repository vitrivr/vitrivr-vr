using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Interface between the <see cref="QueryController"/> and the individual <see cref="QueryTermProvider"/>s.
  /// Manages the <see cref="QueryTerm"/>s into stages and temporal contexts.
  /// </summary>
  public abstract class QueryTermManager : MonoBehaviour
  {

    /// <summary>
    /// Retrieves the <see cref="QueryTerm"/>s sorted into stages and temporal contexts.
    /// </summary>
    /// <returns>List of temporal contexts containing lists of stages containing lists of terms.</returns>
    public abstract List<List<List<QueryTerm>>> GetTerms();
  }
}
using System.Collections.Generic;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public abstract class CanvasBooleanTerm : MonoBehaviour
  {
    public abstract List<(string attribute, RelationalOperator op, string[] values)> GetTerms();
    
    /// <returns>Whether the term is enabled and should be used in queries.</returns>
    public abstract bool IsEnabled();
  }
}
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Query;

namespace VitrivrVR.Query.Term.Boolean
{
  public abstract class CanvasBooleanTerm : MonoBehaviour
  {
    public abstract (string attribute, RelationalOperator op, string[] values) GetTerm();
  }
}
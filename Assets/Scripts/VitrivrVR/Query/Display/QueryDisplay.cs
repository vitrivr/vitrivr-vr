using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Abstract class for displaying queries.
  /// </summary>
  public abstract class QueryDisplay : MonoBehaviour
  {
    public virtual int NumberOfResults => -1;
    public QueryResponse QueryData => queryData;


    protected QueryResponse queryData;

    public void Initialize(QueryResponse queryResult)
    {
      queryData = queryResult;
      Initialize();
    }

    protected abstract void Initialize();
  }
}
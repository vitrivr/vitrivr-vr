using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Abstract class for displaying queries.
  /// </summary>
  public abstract class QueryDisplay : MonoBehaviour
  {
    /// <summary>
    /// Contains supported display modes for query displays.
    /// </summary>
    public enum DisplayMode
    {
      MediaSegmentDisplay,
      MediaObjectDisplay
    }

    public virtual int NumberOfResults => -1;

    public abstract void Initialize(QueryResponse queryData);

    public virtual void SwitchDisplayMode(DisplayMode mode)
    {
    }
  }
}
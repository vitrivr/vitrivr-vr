using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;

namespace VitrivrVR.Query.Display
{
  public abstract class QueryDisplay : MonoBehaviour
  {
    public abstract void Initialize(QueryData queryData);
  }
}
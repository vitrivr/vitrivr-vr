using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;

namespace VitrivrVR.Media
{
  public abstract class MediaItemDisplay : MonoBehaviour
  {
    public abstract Task Initialize(SegmentData segment);
  }
}
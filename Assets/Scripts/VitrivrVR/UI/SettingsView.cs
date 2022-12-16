using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Media.Display;
using VitrivrVR.Query;

namespace VitrivrVR.UI
{
  public class SettingsView : MonoBehaviour
  {
    public TMP_InputField segmentIdField;

    public async void GetSegmentById(string segmentId)
    {
      var segment = QueryController.Instance.GetSegment(segmentId);
      var scoredSegment = new ScoredSegment(segment, 0);
      var t = transform;
      await MediaDisplayFactory.CreateDisplay(scoredSegment, () => { }, t.position - 0.2f * t.forward, t.rotation);
    }


    public void GetSegmentByID()
    {
      GetSegmentById(segmentIdField.text);
    }
  }
}
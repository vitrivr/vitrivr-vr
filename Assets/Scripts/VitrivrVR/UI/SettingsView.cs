using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Media.Display;

namespace VitrivrVR.UI
{
  public class SettingsView : MonoBehaviour
  {
    public TMP_InputField segmentIdField;

    public async void GetSegmentById(string segmentId)
    {
      var segment = SegmentRegistry.GetSegment(segmentId);
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
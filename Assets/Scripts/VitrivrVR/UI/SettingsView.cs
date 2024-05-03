using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Media.Display;
using VitrivrVR.Notification;
using VitrivrVR.Query;
using VitrivrVR.Submission;

namespace VitrivrVR.UI
{
  public class SettingsView : MonoBehaviour
  {
    public TMP_InputField segmentIdField;
    public TMP_Dropdown cineastDropdown;
    public GameObject evaluationIdSelection;
    public TMP_Dropdown evaluationIdDropdown;

    private List<string> _evaluationIds = new();

    private async void Start()
    {
      cineastDropdown.AddOptions(QueryController.Instance.AvailableCineastClients);
      // If DRES is enabled in config, make sure evaluation IDs are updated and add them to dropdown, otherwise destroy the dropdown.
      if (ConfigManager.Config.dresEnabled)
      {
        var evaluations = DresClientManager.GetEvaluations() ?? await DresClientManager.UpdateEvaluations();
        var evaluationsList = evaluations.ToList();

        _evaluationIds = evaluationsList.Select(e => e.Id).ToList();
        evaluationIdDropdown.AddOptions(evaluationsList.Select(e => e.Name).ToList());
      }
      else
      {
        Destroy(evaluationIdSelection);
      }
    }

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

    public void SelectCineast(int cineastIndex)
    {
      QueryController.Instance.SelectCineastClient(cineastIndex);
      NotificationController.Notify($"Switched to \"{cineastDropdown.options[cineastIndex].text}\"");
    }

    public void SelectEvaluationId(int evaluationIdIndex)
    {
      DresClientManager.SetCurrentEvaluation(_evaluationIds[evaluationIdIndex]);
      NotificationController.Notify($"Set evaluation to \"{evaluationIdDropdown.options[evaluationIdIndex].text}\"");
    }

    public async void UpdateEvaluationIds()
    {
      var evaluations = await DresClientManager.UpdateEvaluations();
      var evaluationsList = evaluations.ToList();

      _evaluationIds = evaluationsList.Select(e => e.Id).ToList();
      evaluationIdDropdown.ClearOptions();
      evaluationIdDropdown.AddOptions(evaluationsList.Select(e => e.Name).ToList());
      // Set the first option as selected
      evaluationIdDropdown.value = 0;
    }
  }
}
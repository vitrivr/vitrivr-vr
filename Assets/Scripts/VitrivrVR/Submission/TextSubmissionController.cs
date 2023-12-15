using TMPro;
using UnityEngine;
using VitrivrVR.Config;

namespace VitrivrVR.Submission
{
  public class TextSubmissionController : MonoBehaviour
  {
    private TMP_InputField _submissionText;

    private void Start()
    {
      _submissionText = GetComponentInChildren<TMP_InputField>();

      // Check if is enabled
      var config = ConfigManager.Config;
      gameObject.SetActive(config.dresEnabled && config.textSubmissionEnabled);
    }

    public void SubmitText()
    {
      if (_submissionText.text.Length == 0) return;

      DresClientManager.SubmitTextResult(_submissionText.text);
    }
  }
}
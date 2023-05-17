using System;
using TMPro;
using UnityEngine;

namespace VitrivrVR.Submission
{
  public class TextSubmissionController : MonoBehaviour
  {
    private TMP_InputField _submissionText;

    private void Start()
    {
      _submissionText = GetComponentInChildren<TMP_InputField>();
    }

    public void SubmitText()
    {
      if (_submissionText.text.Length == 0) return;
      
      DresClientManager.SubmitTextResult(_submissionText.text);
    }
  }
}
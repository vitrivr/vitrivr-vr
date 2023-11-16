using System;
using UnityEngine;
using VitrivrVR.Config;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// Manages dictation controllers.
  /// </summary>
  public class DictationManager : MonoBehaviour
  {
    public GameObject deepSpeech;
    public GameObject whisper;

    private void Start()
    {
      switch (ConfigManager.Config.defaultSpeechToText)
      {
        case VitrivrVrConfig.SpeechToText.DeepSpeech:
          deepSpeech.SetActive(true);
          break;
        case VitrivrVrConfig.SpeechToText.Whisper:
          whisper.SetActive(true);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
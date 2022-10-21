using System;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media.Display
{
  /// <summary>
  /// Abstract class for media displays representing a media object.
  /// </summary>
  public abstract class MediaDisplay : MonoBehaviour
  {
    public abstract void Initialize(ScoredSegment scoredSegment, Action onClose);
  }
}
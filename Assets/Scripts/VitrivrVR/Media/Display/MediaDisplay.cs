using System;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Media.Display
{
  public abstract class MediaDisplay : MonoBehaviour
  {
    public abstract void Initialize(ScoredSegment scoredSegment, Action onClose);
  }
}
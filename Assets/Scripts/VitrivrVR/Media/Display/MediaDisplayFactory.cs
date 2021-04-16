using System;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;

namespace VitrivrVR.Media.Display
{
  public class MediaDisplayFactory : MonoBehaviour
  {
    public MediaDisplay videoDisplayPrefab;

    public static MediaDisplayFactory Instance { get; private set; }

    private void Awake()
    {
      if (Instance != null)
      {
        Debug.LogError("Multiple MediaDisplayFactories registered!");
      }

      Instance = this;
    }

    public static async Task<MediaDisplay> CreateDisplay(ScoredSegment scoredSegment, Action onClose, Vector3 position,
      Quaternion rotation)
    {
      if (Instance == null)
      {
        Debug.LogError("No MediaDisplayFactory in scene!");
      }

      return await Instance.Create(scoredSegment, onClose, position, rotation);
    }

    private async Task<MediaDisplay> Create(ScoredSegment scoredSegment, Action onClose, Vector3 position,
      Quaternion rotation)
    {
      var segment = scoredSegment.segment;
      var mediaObject = await ObjectRegistry.GetObjectOf(segment.Id);
      var mediaType = await mediaObject.GetMediaType();

      switch (mediaType)
      {
        case MediaObjectDescriptor.MediatypeEnum.VIDEO:
          var videoDisplay = Instantiate(videoDisplayPrefab, position, rotation);
          videoDisplay.Initialize(scoredSegment, onClose);
          return videoDisplay;
        default:
          Debug.LogError($"Encountered unsupported MediaType: {mediaType}");
          return null;
      }
    }
  }
}
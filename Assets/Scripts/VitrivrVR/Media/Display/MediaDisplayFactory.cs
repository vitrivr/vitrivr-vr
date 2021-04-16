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
    [Serializable]
    public class UnknownMediaTypeException : Exception
    {
      public UnknownMediaTypeException()
      {
      }

      public UnknownMediaTypeException(string message) : base(message)
      {
      }

      public UnknownMediaTypeException(string message, Exception inner) : base(message, inner)
      {
      }

      protected UnknownMediaTypeException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
      {
      }
    }

    [Serializable]
    public class UnsupportedMediaTypeException : Exception
    {
      public UnsupportedMediaTypeException()
      {
      }

      public UnsupportedMediaTypeException(string message) : base(message)
      {
      }

      public UnsupportedMediaTypeException(string message, Exception inner) : base(message, inner)
      {
      }

      protected UnsupportedMediaTypeException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
      {
      }
    }

    public MediaDisplay videoDisplayPrefab;
    public MediaDisplay imageSequenceDisplayPrefab;

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
        case MediaObjectDescriptor.MediatypeEnum.IMAGE:
          throw new NotImplementedException($"{mediaType} support is not yet implemented (oID: {mediaObject.Id})");
        case MediaObjectDescriptor.MediatypeEnum.AUDIO:
          throw new NotImplementedException($"{mediaType} support is not yet implemented (oID: {mediaObject.Id})");
        case MediaObjectDescriptor.MediatypeEnum.MODEL3D:
          throw new NotImplementedException($"{mediaType} support is not yet implemented (oID: {mediaObject.Id})");
        case MediaObjectDescriptor.MediatypeEnum.IMAGESEQUENCE:
          var imageSequenceDisplay = Instantiate(imageSequenceDisplayPrefab, position, rotation);
          imageSequenceDisplay.Initialize(scoredSegment, onClose);
          return imageSequenceDisplay;
        case MediaObjectDescriptor.MediatypeEnum.UNKNOWN:
          throw new UnknownMediaTypeException($"Media object {mediaObject.Id} has unknown MediaType: {mediaType}");
        case null:
          throw new NullReferenceException($"Media object {mediaObject.Id} has no media type!");
        default:
          throw new UnsupportedMediaTypeException(
            $"Media object {mediaObject.Id} has unknown, unsupported MediaType: {mediaType}");
      }
    }
  }
}
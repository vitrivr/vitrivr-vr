using System;
using System.Collections;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Model.Data;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VitrivrVR.Media
{
  public class CanvasMediaItemDisplay : MediaItemDisplay
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;

    private SegmentData _segment;
    private bool _videoInitialized;
    private VideoPlayer _videoPlayer;

    private class ClickHandler : MonoBehaviour, IPointerClickHandler
    {
      public Action onClick;

      public void OnPointerClick(PointerEventData eventData)
      {
        onClick();
      }
    }

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClick;
    }

    public override async Task Initialize(SegmentData segment)
    {
      _segment = segment;
      var config = CineastConfigManager.Instance.Config;
      var objectId = await segment.GetObjectId();
      var thumbnailPath = PathResolver.ResolvePath(config.thumbnailPath, objectId, segment.GetId());
      var thumbnailUrl = $"{config.mediaHost}{thumbnailPath}{config.thumbnailExtension}";
      StartCoroutine(DownloadTexture(thumbnailUrl));
    }

    private void OnClick()
    {
      if (_videoInitialized)
      {
        if (_videoPlayer.isPlaying)
        {
          _videoPlayer.Pause();
        }
        else
        {
          _videoPlayer.Play();
        }
      }
      else
      {
        InitializeVideo();
      }
    }

    private async void InitializeVideo()
    {
      _videoInitialized = true;

      previewImage.texture = loadingTexture;
      previewImage.transform.localScale = Vector3.one;

      var config = CineastConfigManager.Instance.Config;
      var objectId = await _segment.GetObjectId();
      var mediaPath = PathResolver.ResolvePath(config.mediaPath, objectId);
      var mediaUrl = $"{config.mediaHost}{mediaPath}";


      _videoPlayer = gameObject.AddComponent<VideoPlayer>();
      var audioSource = gameObject.AddComponent<AudioSource>();

      _videoPlayer.url = mediaUrl;

      _videoPlayer.isLooping = true;
      _videoPlayer.renderMode = VideoRenderMode.RenderTexture;

      _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      _videoPlayer.SetTargetAudioSource(0, audioSource);
      _videoPlayer.prepareCompleted += PrepareCompleted;
      _videoPlayer.errorReceived += ErrorEncountered;
    }

    private IEnumerator DownloadTexture(string url)
    {
      UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.isNetworkError || www.isHttpError)
      {
        Debug.LogError(www.error);
        previewImage.texture = errorTexture;
      }
      else
      {
        Texture2D loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        previewImage.texture = loadedTexture;
        float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
        Vector3 scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
        previewImage.transform.localScale = scale;
      }
    }

    async void PrepareCompleted(VideoPlayer videoPlayer)
    {
      float factor = Mathf.Max(videoPlayer.width, videoPlayer.height);
      Vector3 scale = new Vector3(videoPlayer.width / factor, videoPlayer.height / factor, 1);
      var renderTex = new RenderTexture((int) videoPlayer.width, (int) videoPlayer.height, 24);
      videoPlayer.targetTexture = renderTex;
      previewImage.texture = renderTex;
      previewImage.transform.localScale = scale;
      _videoPlayer.frame = await _segment.GetStart();
    }

    void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      previewImage.texture = errorTexture;
    }
  }
}
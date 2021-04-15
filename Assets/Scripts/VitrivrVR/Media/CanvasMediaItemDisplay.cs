using System;
using System.Collections;
using System.Threading.Tasks;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using VitrivrVR.Config;
using VitrivrVR.Util;

namespace VitrivrVR.Media
{
  /// <summary>
  /// Canvas based <see cref="MediaItemDisplay"/>.
  /// </summary>
  public class CanvasMediaItemDisplay : MediaItemDisplay
  {
    public Texture2D errorTexture;
    public Texture2D loadingTexture;
    public RawImage previewImage;
    public RawImage scoreFrame;
    public RectTransform imageFrame;
    public int scoreFrameSize = 25;

    public CanvasVideoDisplay canvasVideoDisplay;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private CanvasVideoDisplay _videoDisplay;

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClickImage;
    }

    public override ScoredSegment ScoredSegment => _scoredSegment;

    /// <summary>
    /// Initializes this display with the given segment data.
    /// </summary>
    /// <param name="segment">Segment to display</param>
    public override async Task Initialize(ScoredSegment segment)
    {
      _scoredSegment = segment;
      _segment = segment.segment;
      var vrConfig = ConfigManager.Config;
      var score = (float) _scoredSegment.score;
      // Score frame
      scoreFrame.color = vrConfig.similarityColor.ToColor() * score +
                         vrConfig.dissimilarityColor.ToColor() * (1 - score);
      var rectTransform = scoreFrame.rectTransform;
      rectTransform.offsetMin = new Vector2(-scoreFrameSize, -scoreFrameSize);
      rectTransform.offsetMax = new Vector2(scoreFrameSize, scoreFrameSize);
      scoreFrame.gameObject.SetActive(true);
      try
      {
        var thumbnailUrl = await CineastWrapper.GetThumbnailUrlOfAsync(_segment);
        StartCoroutine(DownloadThumbnailTexture(thumbnailUrl));
      }
      catch (Exception)
      {
        previewImage.texture = errorTexture;
      }
    }

    private void OnClickImage(PointerEventData pointerEventData)
    {
      if (_videoDisplay)
      {
        ClosePopoutVideo();
      }
      else
      {
        var t = transform;
        _videoDisplay = Instantiate(canvasVideoDisplay, t.position - 0.2f * t.forward, t.rotation);
        _videoDisplay.Initialize(_scoredSegment, ClosePopoutVideo);
        previewImage.color = new Color(.2f, .2f, .2f);
      }
    }

    private void ClosePopoutVideo()
    {
      Destroy(_videoDisplay.gameObject);
      _videoDisplay = null;
      if (!previewImage)
        return;
      previewImage.color = Color.white;
    }

    /// <summary>
    /// Method to download and apply the thumbnail texture from the given URL. Start as <see cref="Coroutine"/>.
    /// </summary>
    /// <param name="url">The URL to the thumbnail file</param>
    private IEnumerator DownloadThumbnailTexture(string url)
    {
      var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
      {
        Debug.LogError(www.error);
        previewImage.texture = errorTexture;
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        previewImage.texture = loadedTexture;
        float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
        imageFrame.sizeDelta =
          new Vector2(1000 * loadedTexture.width / factor, 1000 * loadedTexture.height / factor);
      }
    }
  }
}
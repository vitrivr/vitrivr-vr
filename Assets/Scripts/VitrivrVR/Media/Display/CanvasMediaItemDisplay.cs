using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Display
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

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private MediaDisplay _mediaDisplay;

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
        StartCoroutine(DownloadHelper.DownloadTexture(thumbnailUrl, OnDownloadError, OnDownloadSuccess));
      }
      catch (Exception)
      {
        previewImage.texture = errorTexture;
      }
    }

    private async void OnClickImage(PointerEventData pointerEventData)
    {
      if (_mediaDisplay)
      {
        CloseMediaDisplay();
      }
      else
      {
        var t = transform;
        _mediaDisplay = await MediaDisplayFactory.CreateDisplay(_scoredSegment, CloseMediaDisplay,
          t.position - 0.2f * t.forward, t.rotation);
        previewImage.color = new Color(.2f, .2f, .2f);
      }
    }

    private void CloseMediaDisplay()
    {
      Destroy(_mediaDisplay.gameObject);
      _mediaDisplay = null;
      if (!previewImage)
        return;
      previewImage.color = Color.white;
    }

    private void OnDownloadError()
    {
      previewImage.texture = errorTexture;
    }

    private void OnDownloadSuccess(Texture2D loadedTexture)
    {
      previewImage.texture = loadedTexture;
      float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
      imageFrame.sizeDelta =
        new Vector2(1000 * loadedTexture.width / factor, 1000 * loadedTexture.height / factor);
    }
  }
}
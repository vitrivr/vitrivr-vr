using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Submission;
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

    // Action for quick submit
    public InputAction submitAction;

    private ScoredSegment _scoredSegment;
    private SegmentData _segment;
    private MediaDisplay _mediaDisplay;

    private HoverHandler _hoverHandler;
    private bool _hovered;

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
      var clickHandler = previewImage.gameObject.AddComponent<ClickHandler>();
      clickHandler.onClick = OnClickImage;

      _hoverHandler = GetComponent<HoverHandler>();
      _hoverHandler.onEnter += OnHoverEnter;
      _hoverHandler.onExit += OnHoverExit;
      submitAction.performed += Submit;
    }

    private async void Start()
    {
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

    private void OnEnable()
    {
      submitAction.Enable();
    }

    private void OnDisable()
    {
      submitAction.Disable();
      _hovered = false;
    }

    public override ScoredSegment ScoredSegment => _scoredSegment;

    /// <summary>
    /// Initializes this display with the given segment data.
    /// </summary>
    /// <param name="segment">Segment to display</param>
    public override void Initialize(ScoredSegment segment)
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

    private void OnHoverEnter(PointerEventData pointerEventData)
    {
      _hovered = true;
    }

    private void OnHoverExit(PointerEventData pointerEventData)
    {
      _hovered = false;
    }

    private void Submit(InputAction.CallbackContext context)
    {
      if (!_hovered) return;
      if (!ConfigManager.Config.dresEnabled) return;

      DresClientManager.QuickSubmitSegment(_segment);
    }
  }
}
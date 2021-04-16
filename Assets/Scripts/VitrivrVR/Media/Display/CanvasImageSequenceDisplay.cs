using System;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Display
{
  public class CanvasImageSequenceDisplay : MediaDisplay
  {
    public Texture2D errorTexture;
    public RawImage previewImage;

    private ScoredSegment _scoredSegment;
    private SegmentData Segment => _scoredSegment.segment;
    private ObjectData _mediaObject;
    private Action _onClose;

    public override async void Initialize(ScoredSegment scoredSegment, Action onClose)
    {
      _scoredSegment = scoredSegment;
      _onClose = onClose;
      _mediaObject = ObjectRegistry.GetObject(await Segment.GetObjectId());

      // Resolve media URL
      var mediaUrl = await CineastWrapper.GetMediaUrlOfAsync(_mediaObject, Segment.Id);

      StartCoroutine(DownloadHelper.DownloadTexture(mediaUrl,
        () => { previewImage.texture = errorTexture; },
        texture => { previewImage.texture = texture; }
      ));
    }

    public void Close()
    {
      _onClose();
    }
  }
}
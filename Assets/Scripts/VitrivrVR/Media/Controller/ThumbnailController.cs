using UnityEngine;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Controller
{
  /// <summary>
  /// Legacy media display exclusively for thumbnails.
  /// </summary>
  public class ThumbnailController : MonoBehaviour
  {
    public Texture2D errorTexture;
    public string url;
    public float scaleFactor = 1;

    private void Start()
    {
      if (!string.IsNullOrEmpty(url))
      {
        StartDownload();
      }
    }

    public void StartDownload()
    {
      StartCoroutine(DownloadHelper.DownloadTexture(url, OnDownloadError, OnDownloadSuccess));
    }

    private void OnDownloadError()
    {
      var rend = GetComponent<Renderer>();
      rend.material.mainTexture = errorTexture;
    }

    private void OnDownloadSuccess(Texture2D loadedTexture)
    {
      var rend = GetComponent<Renderer>();
      rend.material.mainTexture = loadedTexture;
      float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
      var scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
      rend.transform.localScale = scale * scaleFactor;
    }
  }
}
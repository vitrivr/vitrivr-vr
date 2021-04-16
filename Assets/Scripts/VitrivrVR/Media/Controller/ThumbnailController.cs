using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VitrivrVR.Media.Controller
{
  /// <summary>
  /// Legacy media display exclusively for thumbnails.
  /// </summary>
  public class ThumbnailController : MonoBehaviour
  {
    public Texture2D errorTexture;
    public string url;

    private void Start()
    {
      var renderer = GetComponent<Renderer>();
      StartCoroutine(DownloadTexture(renderer));
    }

    private IEnumerator DownloadTexture(Renderer renderer)
    {
      var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
      {
        Debug.LogError($"{url}\n{www.error}");
        renderer.material.mainTexture = errorTexture;
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        renderer.material.mainTexture = loadedTexture;
        float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
        var scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
        renderer.transform.localScale = scale;
      }
    }
  }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VitrivrVR.Util
{
  public static class DownloadHelper
  {
    public static IEnumerator DownloadTexture(string url, Action onError, Action<Texture2D> onSuccess)
    {
      using var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
      {
        Debug.LogError(www.error);
        onError();
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        onSuccess(loadedTexture);
      }
    }
  }
}
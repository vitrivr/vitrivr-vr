using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VitrivrVR.Media
{
    public class ThumbnailController : MonoBehaviour
    {
        public Texture2D errorTexture;
        public string URL { get; set; }

        void Start()
        {
            Renderer renderer = GetComponent<Renderer>();
            // TODO: It may be worth loading thumbnails in a separate thread rather than coroutine
            StartCoroutine(DownloadTexture(URL, renderer));
        }

        IEnumerator DownloadTexture(string url, Renderer renderer)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                renderer.material.mainTexture = errorTexture;
            }
            else
            {
                Texture2D loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
                renderer.material.mainTexture = loadedTexture;
                float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
                Vector3 scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
                renderer.transform.localScale = scale;
            }
        }
    }
}
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VitrivrVR.Util
{
  /// <summary>
  /// Class to help take screenshots with transparent backgrounds.
  /// </summary>
  public class ScreenshotHelper : MonoBehaviour
  {
    public InputAction screenshotAction;
    public int width;
    public int height;
    public string savePath;

    private void Start()
    {
      screenshotAction.performed += _ => TakeTransparentScreenshot();
    }

    private void OnEnable()
    {
      screenshotAction.Enable();
    }

    private void OnDisable()
    {
      screenshotAction.Disable();
    }

    public void TakeTransparentScreenshot()
    {
      var currentDateTime = System.DateTime.Now;
      var dateTimeString = currentDateTime.ToString("yyyy-MM-dd_HH-mm-ss");
      var filename = $"screenshot-{dateTimeString}.png";
      var path = string.IsNullOrEmpty(savePath) ? filename : Path.Combine(savePath, filename);
      TakeTransparentScreenshot(Camera.main, width, height, path);
    }

    public static void TakeTransparentScreenshot(Camera cam, int width, int height, string savePath)
    {
      // Depending on your render pipeline, this may not work.
      var bakCamTargetTexture = cam.targetTexture;
      var bakCamClearFlags = cam.clearFlags;
      var bakRenderTextureActive = RenderTexture.active;

      var texTransparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
      // Must use 24-bit depth buffer to be able to fill background.
      var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
      var grabArea = new Rect(0, 0, width, height);

      RenderTexture.active = renderTexture;
      cam.targetTexture = renderTexture;
      cam.clearFlags = CameraClearFlags.SolidColor;

      // Simple: use a clear background
      cam.backgroundColor = Color.clear;
      cam.Render();
      texTransparent.ReadPixels(grabArea, 0, 0);
      texTransparent.Apply();

      // Encode the resulting output texture to a byte array then write to the file
      var pngShot = texTransparent.EncodeToPNG();
      File.WriteAllBytes(savePath, pngShot);

      cam.clearFlags = bakCamClearFlags;
      cam.targetTexture = bakCamTargetTexture;
      RenderTexture.active = bakRenderTextureActive;
      RenderTexture.ReleaseTemporary(renderTexture);
      Destroy(texTransparent);
    }

    private void OnDrawGizmos()
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(1.6f * Vector3.up, .2f);
      Gizmos.DrawWireCube(.7f * Vector3.up, new Vector3(.5f, 1.4f, .3f));
    }
  }
}
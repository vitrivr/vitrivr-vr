using UnityEngine;

namespace VitrivrVR.Util
{
  public class ImageUtils
  {
    public static Texture2D ResampleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
      var sourceAspect = source.width / (float)source.height;
      var targetAspect = targetWidth / (float)targetHeight;

      var newWidth = targetWidth;
      var newHeight = targetHeight;

      if (sourceAspect > targetAspect)
      {
        // Width will be the limiting factor
        newHeight = (int)(targetWidth / sourceAspect);
      }
      else
      {
        // Height will be the limiting factor
        newWidth = (int)(targetHeight * sourceAspect);
      }

      var resampled = new Texture2D(newWidth, newHeight, source.format, false);

      var colors = new Color[newWidth * newHeight];

      for (var y = 0; y < newHeight; y++)
      {
        for (var x = 0; x < newWidth; x++)
        {
          colors[y * newWidth + x] = source.GetPixelBilinear(x / (float)newWidth, y / (float)newHeight);
        }
      }

      resampled.SetPixels(colors);
      resampled.Apply();

      return resampled;
    }
  }
}
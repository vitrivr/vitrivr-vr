using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Interaction.System;
using VitrivrVR.Logging;
using VitrivrVR.Media.Display;
using static VitrivrVR.Logging.Interaction;

namespace VitrivrVR.Query.Display
{
  public enum Coloration
  {
    White,
    Coordinates,
    Score
  }

  public class PointCloudDisplay : MonoBehaviour
  {
    public ParticleSystem system;
    public Renderer previewPrefab;
    public float previewScale = 0.2f;
    public Coloration startColor;

    [Tooltip("Maximum distance (squared) to point such that thumbnail is still displayed")]
    public float maximumDistanceSquared = 0.01f;

    [Header("Randomized Previews")] public bool enableRandomizedPreviews = true;

    [Tooltip("Minimum distance between previews to control sparsity")]
    public float minimumPreviewDistance = .18f;

    public float randomizedPreviewScale = .1f;
    public float hideRandomizedPreviewDistanceSquared = 0.02f;

    private Dictionary<Vector3, Renderer> _randomizedPreviews = new();

    private List<(SegmentData segment, Vector3 position, float score)> _points = new();

    // Interaction variables
    // For tracking scaling events.
    private readonly Dictionary<Transform, Vector3> _activeInteractors = new();
    private bool _scalePerformed;

    // For tracking the interactors currently inside the bounding box and the last segment data they hovered.
    private readonly Dictionary<Interactor, SegmentData> _enteredLastSegment = new();
    private readonly Dictionary<Interactor, Renderer> _enteredPreviews = new();

    private Camera _camera;


    private void Start()
    {
      _camera = Camera.main;
      LoggingController.LogInteraction("exploration", $"opened point cloud display", QueryManagement);
    }

    private void Update()
    {
      UpdateInteraction();
    }

    private void FixedUpdate()
    {
      UpdatePreviews();
    }

    private void OnDisable()
    {
      foreach (var preview in _randomizedPreviews.Values.Where(preview => preview != null))
      {
        preview.gameObject.SetActive(false);
      }

      foreach (var preview in _enteredPreviews.Values)
      {
        preview.enabled = false;
      }
    }

    private void OnEnable()
    {
      foreach (var preview in _randomizedPreviews.Values)
      {
        preview.gameObject.SetActive(true);
      }

      if (_points.Count > 0)
        EmitParticles(startColor);
    }

    private void OnDestroy()
    {
      foreach (var preview in _randomizedPreviews.Values)
      {
        Destroy(preview.gameObject);
      }
      LoggingController.LogInteraction("exploration", $"closed point cloud display", QueryManagement);
    }

    public void Initialize(List<(SegmentData segment, Vector3 position, float score)> items)
    {
      items = NormalizeToBoundingBox(items);

      var mainConfig = system.main;
      mainConfig.maxParticles = items.Count;

      _points = items;

      EmitParticles(startColor);

      if (enableRandomizedPreviews)
        GeneratePreviews();
    }

    public void OnInteraction(Transform interactor, bool start)
    {
      if (start)
      {
        _activeInteractors.Add(interactor, interactor.position);
      }
      else
      {
        _activeInteractors.Remove(interactor);

        if (_activeInteractors.Count > 0)
          return;

        // Check if scaling event occurred
        if (!_scalePerformed)
          SelectPoint(interactor);

        // Reset scale performed
        _scalePerformed = false;
      }
    }

    private async void SelectPoint(Transform interactor)
    {
      var position = transform.InverseTransformPoint(interactor.position);

      var (segment, sqrDistance, itemPosition, score) = _points
        .Select(item => (item.segment, (item.position - position).sqrMagnitude, item.position, item.score))
        .Aggregate((a, b) => a.sqrMagnitude > b.sqrMagnitude ? b : a);

      var interactorClose = sqrDistance < maximumDistanceSquared / transform.localScale.x;

      if (!interactorClose)
        return;

      var worldPosition = transform.TransformPoint(itemPosition);

      await MediaDisplayFactory.CreateDisplay(new ScoredSegment(segment, score), () => { }, worldPosition,
        Quaternion.LookRotation(worldPosition - _camera.transform.position));
      LoggingController.LogInteraction("exploration", $"selected {segment.Id} {interactor.name}",
        ResultExpansion);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (!other.TryGetComponent<Interactor>(out var interactor)) return;

      // Check if interactor had previously entered but not been removed
      if (_enteredLastSegment.ContainsKey(interactor))
      {
        _enteredLastSegment[interactor] = null;
        return;
      }

      _enteredLastSegment.Add(interactor, null);

      var preview = Instantiate(previewPrefab);
      preview.transform.localScale = previewScale * Vector3.one;
      _enteredPreviews.Add(interactor, preview);
    }

    private void OnTriggerExit(Collider other)
    {
      if (!other.TryGetComponent<Interactor>(out var interactor)) return;

      _enteredLastSegment.Remove(interactor);

      var preview = _enteredPreviews[interactor];
      Destroy(preview.gameObject);
      _enteredPreviews.Remove(interactor);
    }

    private void EmitParticles(Coloration coloration)
    {
      system.Clear();

      // Scale particles by size to allow scaling of point cloud for easier selection and viewing, but do not scale up
      // beyond a limit to prevent small displays becoming crowded
      var size = .01f / Mathf.Max(transform.localScale.x, 1);

      switch (coloration)
      {
        case Coloration.White:
          foreach (var emitParams in _points.Select(item => new ParticleSystem.EmitParams
                   {
                     position = item.position,
                     velocity = Vector3.zero,
                     startLifetime = float.PositiveInfinity,
                     startSize = size,
                     startColor = Color.white
                   }))
          {
            system.Emit(emitParams, 1);
          }

          break;
        case Coloration.Coordinates:
          foreach (var emitParams in _points.Select(item => new ParticleSystem.EmitParams
                   {
                     position = item.position,
                     velocity = Vector3.zero,
                     startLifetime = float.PositiveInfinity,
                     startSize = size,
                     startColor = new Color(item.position.x + 0.5f, item.position.y + 0.5f, item.position.z + 0.5f)
                   }))
          {
            system.Emit(emitParams, 1);
          }

          break;
        case Coloration.Score:
          // Find min and max score
          var scores = _points.Select(item => item.score).ToArray();
          var minScore = scores.Min();
          var maxScore = scores.Max();
          var range = maxScore - minScore;

          var similarityColor = ConfigManager.Config.similarityColor.ToColor();
          var dissimilarityColor = ConfigManager.Config.dissimilarityColor.ToColor();

          foreach (var emitParams in _points.Select(item => new ParticleSystem.EmitParams
                   {
                     position = item.position,
                     velocity = Vector3.zero,
                     startLifetime = float.PositiveInfinity,
                     startSize = size,
                     startColor = (item.score - minScore) / range * similarityColor +
                                  (1 - (item.score - minScore) / range) * dissimilarityColor
                   }))
          {
            system.Emit(emitParams, 1);
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(coloration), coloration, null);
      }
    }

    private async void GeneratePreviews()
    {
      // Destroy existing previews
      foreach (var (_, preview) in _randomizedPreviews)
      {
        Destroy(preview.gameObject);
      }

      _randomizedPreviews.Clear();

      // Try to favor points with higher scores as previews
      var orderedPoints = _points.OrderByDescending(point => point.score);

      var previewPoints = new List<(SegmentData segment, Vector3 position)>();

      var sqrMinimumPreviewDistance = minimumPreviewDistance * minimumPreviewDistance;

      foreach (var (segment, position, _) in orderedPoints)
      {
        if (previewPoints.Count == 0)
        {
          previewPoints.Add((segment, position));
          continue;
        }

        var minSqrDistance = previewPoints.Select(pair => (pair.position - position).sqrMagnitude).Min();

        // Ignore points too close to existing previews
        if (minSqrDistance < sqrMinimumPreviewDistance)
          continue;

        previewPoints.Add((segment, position));
      }

      // Create previews
      _randomizedPreviews = previewPoints.Select(pair =>
      {
        var preview = Instantiate(previewPrefab);
        preview.transform.localScale = previewScale * Vector3.one;

        return (preview, pair.position);
      }).ToDictionary(pair => pair.position, pair => pair.preview);

      // Start texture download (must occur after previews are created)
      foreach (var (segment, position) in previewPoints)
      {
        var thumbnailURL = await segment.GetThumbnailUrl();

        StartCoroutine(DownloadTexture(thumbnailURL, segment.Id, position, null,
          OnDownloadSuccessRandomizedPreview));
      }
    }

    private void UpdateInteraction()
    {
      if (_activeInteractors.Count != 2) return;

      _scalePerformed = true;

      var positions = _activeInteractors.Keys.Select(key => (_activeInteractors[key], key.position)).ToArray();

      var (old0, new0) = positions.First();
      var (old1, new1) = positions.Last();

      var t = transform;
      var scalingFactor = (new0 - new1).magnitude / (old0 - old1).magnitude;
      // Perform scaling at the center point of the gesture
      var position = t.position;
      var scalingPoint = (old0 + old1) / 2 - position;

      t.localScale *= scalingFactor;
      position += scalingPoint - scalingPoint * scalingFactor;
      t.position = position;

      var keys = _activeInteractors.Keys.ToArray();
      foreach (var key in keys)
      {
        _activeInteractors[key] = key.position;
      }

      EmitParticles(startColor);
    }

    /// <summary>
    /// Determine which point is closest to the preview position and start the necessary coroutine to download the
    /// appropriate texture.
    /// Also updates preview location and rotation.
    /// </summary>
    private async void UpdatePreviews()
    {
      if (_points.Count == 0)
        return;

      // Enable all randomized previews
      _randomizedPreviews.Values.ToList().ForEach(preview => preview.enabled = true);

      // Update interactor previews
      foreach (var interactor in _enteredLastSegment.Keys.ToList())
      {
        var position = transform.InverseTransformPoint(interactor.transform.position);

        var (segment, sqrDistance, itemPosition) = _points
          .Select(item => (item.segment, (item.position - position).sqrMagnitude, item.position))
          .Aggregate((a, b) => a.sqrMagnitude > b.sqrMagnitude ? b : a);

        var preview = _enteredPreviews[interactor];

        var interactorClose = sqrDistance < maximumDistanceSquared / transform.localScale.x;

        preview.enabled = interactorClose;

        if (!interactorClose)
          continue;

        // Disable all randomized previews closer to the interactor than the minimum distance
        _randomizedPreviews.Where(pair =>
            (pair.Key - position).sqrMagnitude < hideRandomizedPreviewDistanceSquared / transform.localScale.x)
          .ToList().ForEach(pair => pair.Value.enabled = false);

        UpdatePreviewPosRot(itemPosition, preview.transform);

        if (segment == _enteredLastSegment[interactor])
          continue;

        var thumbnailURL = await segment.GetThumbnailUrl();

        StartCoroutine(DownloadTexture(thumbnailURL, segment.Id, itemPosition, interactor, OnDownloadSuccess));

        _enteredLastSegment[interactor] = segment;
      }

      // Update randomized preview positions and rotations
      foreach (var (position, preview) in _randomizedPreviews)
      {
        UpdatePreviewPosRot(position, preview.transform);
      }
    }

    /// <summary>
    /// Normalizes points to fit into a cube of side length 1 centered around the origin.
    /// </summary>
    private static List<(SegmentData segment, Vector3 position, float score)> NormalizeToBoundingBox(
      IReadOnlyCollection<(SegmentData segment, Vector3 position, float score)> points)
    {
      var positions = points.Select(item => item.position).ToArray();
      var x = positions.Select(point => point.x).ToArray();
      var y = positions.Select(point => point.y).ToArray();
      var z = positions.Select(point => point.z).ToArray();
      var xMin = x.Min();
      var xMax = x.Max();
      var yMin = y.Min();
      var yMax = y.Max();
      var zMin = z.Min();
      var zMax = z.Max();

      // Length of the largest size of the bounding box
      var normalizer = Mathf.Max(xMax - xMin, yMax - yMin, zMax - zMin);

      // Center of the bounding box
      var center = new Vector3(xMin + xMax, yMin + yMax, zMin + zMax) / 2;

      return points.Select(point => (point.segment, (point.position - center) / normalizer, point.score)).ToList();
    }

    /// <summary>
    /// Downloads the image from the given URL and transforms it into a texture.
    /// 
    /// Checks if this texture is still current using the provided ID.
    /// </summary>
    /// <param name="url">URL of the image to use as texture</param>
    /// <param name="id">ID of the image to be downloaded for checking relevance</param>
    /// <param name="itemPosition">Position of the item in local space</param>
    /// <param name="interactor">Interactor causing this preview to be shown</param>
    /// <param name="onSuccess">Function to call when successfully downloaded</param>
    private static IEnumerator DownloadTexture(string url, string id, Vector3 itemPosition, Interactor interactor,
      Action<Texture2D, string, Vector3, Interactor> onSuccess)
    {
      using var www = UnityWebRequestTexture.GetTexture(url);
      yield return www.SendWebRequest();

      if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
      {
        Debug.LogError($"URL: {url}\nError: {www.error}");
      }
      else
      {
        var loadedTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
        onSuccess(loadedTexture, id, itemPosition, interactor);
      }
    }

    private void OnDownloadSuccess(Texture2D loadedTexture, string id, Vector3 itemPosition, Interactor interactor)
    {
      if (!_enteredLastSegment.ContainsKey(interactor) || id != _enteredLastSegment[interactor].Id)
        return;

      var preview = _enteredPreviews[interactor];
      // Set texture
      preview.material.mainTexture = loadedTexture;
      // Adjust aspect ratio
      float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
      var scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
      var t = preview.transform;
      t.localScale = scale * previewScale;
      UpdatePreviewPosRot(itemPosition, t);
    }

    private void OnDownloadSuccessRandomizedPreview(Texture2D loadedTexture, string id, Vector3 itemPosition,
      Interactor _)
    {
      var preview = _randomizedPreviews[itemPosition];
      // Set texture
      preview.material.mainTexture = loadedTexture;
      // Adjust aspect ratio
      float factor = Mathf.Max(loadedTexture.width, loadedTexture.height);
      var scale = new Vector3(loadedTexture.width / factor, loadedTexture.height / factor, 1);
      var t = preview.transform;
      t.localScale = scale * randomizedPreviewScale;
      UpdatePreviewPosRot(itemPosition, t);
    }

    private void UpdatePreviewPosRot(Vector3 itemPosition, Transform previewTransform)
    {
      // Set position
      previewTransform.position =
        transform.TransformPoint(itemPosition) + Vector3.up * (previewTransform.localScale.y / 2);
      // Rotate towards camera
      var forwardVector = previewTransform.position - _camera.transform.position;
      forwardVector.y = 0;
      previewTransform.rotation = Quaternion.LookRotation(forwardVector);
    }
  }
}
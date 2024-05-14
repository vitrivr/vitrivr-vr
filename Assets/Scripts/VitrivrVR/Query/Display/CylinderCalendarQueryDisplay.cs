using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;
using UnityEngine.InputSystem;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Logging;
using VitrivrVR.Media.Display;
using VitrivrVR.Notification;
using static VitrivrVR.Logging.Interaction;
using DateTime = System.DateTime;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Displays queries as if on the surface of a cylinder in a grid.
  /// Calendar version of object version
  /// </summary>
  public class CylinderCalendarDisplay : QueryDisplay
  {
    public MediaItemDisplay mediaItemDisplay;
    public Transform mediaObjectItemDisplay;
    public int rows = 4;
    public float rotationSpeed = 90;
    public float distance = 1;
    public float resultSize = 0.2f;
    public float padding = 0.02f;

    public InputAction rotationAction;

    public int maxSegmentsPerObject = 3;

    public override int NumberOfResults => _nResults;

    private const float SegmentDistance = .3f;

    /// <summary>
    /// Contains the months from the list of months to instantiate
    /// </summary>
    private readonly Queue<DateTime> _instantiationQueue = new();

    private List<List<ScoredSegment>> _results;

    private int _enqueued;

    private int _instantiated;

    private readonly List<Transform> _mediaObjectMonthDisplays = new();

    private int _nResults;
    private float _columnAngle;
    private int _maxColumns;

    private float _currentRotation;
    private int _currentStart;
    private int _currentEnd;

    private Dictionary<string, DateTime> _idToDate;
    private List<DateTime> _months = new();

    private void Awake()
    {
      var angle = 2 * Mathf.Atan((resultSize + padding) / (2 * distance));
      _maxColumns = Mathf.FloorToInt(2 * Mathf.PI / angle);

      if (_maxColumns * rows > ConfigManager.Config.maxDisplay)
      {
        _maxColumns = ConfigManager.Config.maxDisplay / rows;
      }

      _columnAngle = Mathf.Rad2Deg * (2 * Mathf.PI / _maxColumns);

      if (ConfigManager.Config.reduceMotion)
      {
        rotationAction.started += context =>
        {
          var value = context.ReadValue<Vector2>().x;
          if (value != 0)
          {
            Rotate(ConfigManager.Config.reduceMotionAngle * Mathf.Sign(value));
          }
        };
      }
    }

    private void OnEnable()
    {
      rotationAction.Enable();
    }

    private void OnDisable()
    {
      rotationAction.Disable();
    }

    private void Update()
    {
      if (ConfigManager.Config.reduceMotion)
      {
        Rotate(0); // To ensure update
      }
      else
      {
        Rotate(Time.deltaTime * rotationSpeed * rotationAction.ReadValue<Vector2>().x);
      }

      if (_instantiationQueue.Count > 0)
      {
        CreateMonth(_instantiationQueue.Dequeue());
      }
    }

    protected override async void Initialize()
    {
      var fusionResults = QueryData.GetMeanFusionResults();
      if (fusionResults == null)
      {
        NotificationController.Notify("No results returned from query!");
        fusionResults = new List<ScoredSegment>();
      }

      // Create set of id's from the results
      var ids = fusionResults.Select(segment => segment.segment.Id).ToHashSet();

      var meta = await QueryClient.MiscApi.SelectFromTableByIdsAsync(new SelectByIdsSpecification("feature_lsc_meta",
        new List<string> { "id", "year", "month", "day" }, "id", ids.ToList()));
      // Create list of tuples with id and date and filter by the IDs in the results
      var parsedMeta = meta.Columns.Select(item =>
      {
        var sid = item["id"];
        var year = int.Parse(item["year"]);
        var month = int.Parse(item["month"]);
        var day = int.Parse(item["day"]);
        var date = new DateTime(year, month, day);
        return (sid, date);
      }).ToList();

      Debug.Log($"Parsed {parsedMeta.Count} meta entries for {ids.Count} results");

      // Sort by date
      var sortedMeta = parsedMeta.OrderBy(tuple => tuple.date).ToList();

      // Create sorted list of months for which at least one result exists
      _months = sortedMeta.Select(tuple => new DateTime(tuple.date.Year, tuple.date.Month, 1)).Distinct().ToList();

      _idToDate = sortedMeta.ToDictionary(tuple => tuple.sid, tuple => tuple.date);

      var resultsWithObjectIds = await Task.WhenAll(fusionResults
        .Where(segment => segment.segment.Initialized) // Prevents erroneous segments from preventing initialization
        .Select(async segment => (segment, await segment.segment.GetObjectId()))
      );

      _results = (
        from tuple in resultsWithObjectIds
        group tuple.segment by tuple.Item2
        into resultGroup
        orderby resultGroup.First().score descending
        select resultGroup.ToList()
      ).ToList();

      _nResults = _results.Count;
      // Only instantiate results within the first 3 months
      foreach (var month in _months.Take(3))
      {
        _instantiationQueue.Enqueue(month);
        _enqueued++;
      }

      LoggingController.LogQueryResults("object", fusionResults, QueryData);
    }

    /// <summary>
    /// Rotates the display and ensures that only displays that are supposed to be within the viewing window are
    /// visible. Adds uninitialized segments to the instantiation queue as needed.
    /// </summary>
    private void Rotate(float degrees)
    {
      transform.Rotate(degrees * Vector3.up);
      _currentRotation -= degrees;
      // Subtract 90 from current rotation to have results replaced on the side and not in front of the user
      var rawColumnIndex = Mathf.FloorToInt((_currentRotation - 90) / _columnAngle);

      // Check instantiations
      var enabledEnd = rawColumnIndex + _maxColumns;
      if (enabledEnd / 8 > _mediaObjectMonthDisplays.Count && _enqueued == _instantiated)
      {
        var index = _instantiated;
        if (index < _months.Count)
        {
          _instantiationQueue.Enqueue(_months[index]);
          _enqueued++;
        }
      }

      // Check enabled
      var enabledStart = Math.Max(rawColumnIndex, 0);
      if (enabledStart == _currentStart && enabledEnd == _currentEnd) return;

      // Divide number of columns by 8 (7 days + 1 for padding) to get the number of months to display
      var startMonth = Mathf.Max(Mathf.Min(enabledStart, _currentStart) / 8, 0);
      var endMonth = Mathf.Min(Mathf.Max(enabledEnd, _currentEnd) / 8, _mediaObjectMonthDisplays.Count);
      for (var i = startMonth; i < endMonth; i++)
      {
        // Subtract 1 from enabledEnd to compensate for the last padding column
        var active = enabledStart / 8 <= i && i < (enabledEnd - 1) / 8;
        _mediaObjectMonthDisplays[i].gameObject.SetActive(active);
      }

      _currentStart = enabledStart;
      _currentEnd = enabledEnd;

      LoggingController.LogInteraction("rankedList", $"browse {Mathf.Sign(degrees)}", Browsing);
    }

    private Transform CreateResultObject(List<ScoredSegment> objectResult)
    {
      // Instantiate drawer like display holder
      var mediaObjectDisplay = Instantiate(mediaObjectItemDisplay, Vector3.zero, Quaternion.identity, transform);
      var date = _idToDate[objectResult.First().segment.Id];
      var (position, rotation) = GetResultLocalPosRot(date);

      // Set position and rotation
      mediaObjectDisplay.localPosition = position;
      mediaObjectDisplay.localRotation = rotation;
      // Adjust size
      mediaObjectDisplay.localScale *= resultSize;

      // Get the grab enabled display parent to instantiate segment displays into
      var displayParent = mediaObjectDisplay.GetChild(0);

      // Ensure only the set maximum of segments is displayed
      var enumeratedSegments = objectResult
        .Take(maxSegmentsPerObject)
        .Select((scoredSegment, i) => (scoredSegment, i))
        .ToList();

      // Create segment displays
      enumeratedSegments.ForEach(pair =>
      {
        var (scoredSegment, i) = pair;
        var itemDisplay = Instantiate(mediaItemDisplay, displayParent);

        // Adjust local position based on index and set local rotation to identity
        var t = itemDisplay.transform;
        t.localPosition = Vector3.forward * (i * SegmentDistance);
        t.localRotation = Quaternion.identity;

        itemDisplay.Initialize(scoredSegment);
      });

      // Set grab enabled bounding box size
      if (!displayParent.TryGetComponent<BoxCollider>(out var boxCollider))
        throw new Exception("Could not get BoxCollider!");
      var size = boxCollider.size;
      var center = boxCollider.center;

      size.z = enumeratedSegments.Count * SegmentDistance;
      center.z = (size.z - SegmentDistance) / 2;

      boxCollider.size = size;
      boxCollider.center = center;

      // Set disabled if outside of active range
      mediaObjectDisplay.gameObject.SetActive(true);

      return mediaObjectDisplay;
    }

    private void CreateMonth(DateTime month)
    {
      var calendarDisplay = new GameObject(month.ToString("MMMM yyyy")).transform;
      // Set this object as the parent of the month
      calendarDisplay.SetParent(transform);
      calendarDisplay.localRotation = Quaternion.identity;
      foreach (var objectSegments in _results.Where(result =>
                 _idToDate.ContainsKey(result.First().segment.Id) &&
                 _idToDate[result.First().segment.Id].Year == month.Year &&
                 _idToDate[result.First().segment.Id].Month == month.Month))
      {
        var day = CreateResultObject(objectSegments);
        day.SetParent(calendarDisplay);
      }

      _instantiated++;
      _mediaObjectMonthDisplays.Add(calendarDisplay);
    }

    /// <summary>
    /// Calculates and returns the local position and rotation of a result display based on its index.
    /// </summary>
    private (Vector3 position, Quaternion rotation) GetResultLocalPosRot(DateTime date)
    {
      // Calculate months since earliest date
      var firstMonth = _months.First();
      var months = (date.Year - firstMonth.Year) * 12 + date.Month - firstMonth.Month;
      // Get offset for first weekday of month
      var offset = new DateTime(date.Year, date.Month, 1).DayOfWeek switch
      {
        DayOfWeek.Monday => 0,
        DayOfWeek.Tuesday => 1,
        DayOfWeek.Wednesday => 2,
        DayOfWeek.Thursday => 3,
        DayOfWeek.Friday => 4,
        DayOfWeek.Saturday => 5,
        DayOfWeek.Sunday => 6,
        _ => throw new ArgumentOutOfRangeException()
      };

      var row = 5 - (date.Day - 1 + offset) / 7;
      var column = months * 8 + date.DayOfWeek switch
      {
        DayOfWeek.Monday => 0,
        DayOfWeek.Tuesday => 1,
        DayOfWeek.Wednesday => 2,
        DayOfWeek.Thursday => 3,
        DayOfWeek.Friday => 4,
        DayOfWeek.Saturday => 5,
        DayOfWeek.Sunday => 6,
        _ => throw new ArgumentOutOfRangeException()
      };
      var multiplier = resultSize + padding;
      var position = new Vector3(0, multiplier * row, distance);
      var rotation = Quaternion.Euler(0, column * _columnAngle, 0);
      position = rotation * position;

      return (position, rotation);
    }
  }
}
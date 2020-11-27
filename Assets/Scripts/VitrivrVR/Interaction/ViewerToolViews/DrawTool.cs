using UnityEngine;
using VitrivrVR.Input.Controller;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class DrawTool : ViewerToolView
  {
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    public float maxSegmentDistance = 0.02f;
    public float minCornerAngle = 10;

    private XRButtonObserver _buttonObserver;
    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    private float _sqrMaxSegmentDistance;

    private void Awake()
    {
      _sqrMaxSegmentDistance = maxSegmentDistance * maxSegmentDistance;
      _buttonObserver = FindObjectOfType<XRButtonObserver>();
      if (!_buttonObserver)
      {
        Debug.LogError("Could not find required XRButtonObserver in scene!");
      }

      var go = new GameObject("DrawLine", typeof(LineRenderer));
      _lineGameObject = go.GetComponent<LineRenderer>();
      _lineGameObject.material = lineMaterial;
      _lineGameObject.widthMultiplier = lineWidth;
      _lineGameObject.numCapVertices = 4;
      _lineGameObject.numCornerVertices = 4;
    }

    private void Update()
    {
      if (_currentLine)
      {
        // Update current position
        var numPositions = _currentLine.positionCount;
        var position = transform.position;
        _currentLine.SetPosition(numPositions - 1, position);
        // If the last position was far enough away, create new point
        var lastPosition = _currentLine.GetPosition(numPositions - 2);
        if ((lastPosition - position).sqrMagnitude > _sqrMaxSegmentDistance)
        {
          // Check if the last position can be removed
          if (numPositions > 2)
          {
            var rootPosition = _currentLine.GetPosition(numPositions - 3);
            var angle = Vector3.Angle(lastPosition - rootPosition, position - lastPosition);
            if (angle < minCornerAngle)
            {
              _currentLine.SetPosition(numPositions - 2, position);
            }
            else
            {
              _currentLine.positionCount++;
              _currentLine.SetPosition(numPositions, position);
            }
          }
          else
          {
            _currentLine.positionCount++;
            _currentLine.SetPosition(numPositions, position);
          }
        }
      }
    }

    private void OnEnable()
    {
      _buttonObserver.secondaryButtonEvent.AddListener(OnButton);
    }

    private void OnDisable()
    {
      _buttonObserver.secondaryButtonEvent.RemoveListener(OnButton);
    }

    private void OnButton(bool state)
    {
      if (state)
      {
        _currentLine = Instantiate(_lineGameObject);
        var position = transform.position;
        _currentLine.SetPosition(0, position);
        _currentLine.SetPosition(1, position);
      }
      else
      {
        _currentLine = null;
      }
    }
  }
}
using UnityEngine;
using VitrivrVR.Input.Controller;

namespace VitrivrVR.Interaction.ViewerToolViews
{
  public class DrawTool : ViewerToolView
  {
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    
    private XRButtonObserver _buttonObserver;
    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    
    private void Awake()
    {
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
        _currentLine.SetPosition(1, transform.position);
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

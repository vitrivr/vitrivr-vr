using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VitrivrVR.Interaction.ViewerToolViews;

namespace VitrivrVR.Interaction
{
  public class ViewerToolController : MonoBehaviour
  {
    public float axisDeadZone = 0.1f;

    private ViewerToolView[] _views;
    private int _currentView;
    private bool _justSwitched;
    private XRRayInteractor _rayInteractor;
    private XRInteractorLineVisual _rayRenderer;

    private void Start()
    {
      _rayInteractor = GetComponent<XRRayInteractor>();
      _rayRenderer = GetComponent<XRInteractorLineVisual>();
      _views = GetComponentsInChildren<ViewerToolView>(true);
      if (_views.Length > 0)
      {
        SetCurrentViewActive(true);
      }
    }

    public void AxisInput(Vector2 axis)
    {
      if (_justSwitched)
      {
        if (axis.Equals(Vector2.zero))
        {
          _justSwitched = false;
        }
      }
      else
      {
        if (axis.x > axisDeadZone)
        {
          NextTool();
        }
        else if (axis.x < -axisDeadZone)
        {
          PreviousTool();
        }
      }
    }

    public void NextTool()
    {
      SetCurrentViewActive(false);
      _currentView++;
      _currentView %= _views.Length;
      SetCurrentViewActive(true);
      _justSwitched = true;
    }

    public void PreviousTool()
    {
      SetCurrentViewActive(false);
      _currentView--;
      _currentView += _views.Length;
      _currentView %= _views.Length;
      SetCurrentViewActive(true);
      _justSwitched = true;
    }

    private void SetCurrentViewActive(bool active)
    {
      var view = _views[_currentView];
      view.gameObject.SetActive(active);
      _rayInteractor.enabled = view.EnableRayInteractor;
      _rayRenderer.enabled = view.EnableRayInteractor;
    }
  }
}
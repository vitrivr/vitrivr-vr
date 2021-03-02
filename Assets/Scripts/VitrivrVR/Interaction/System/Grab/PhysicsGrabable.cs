using UnityEngine;

namespace VitrivrVR.Interaction.System.Grab
{
  public class PhysicsGrabable : Grabable
  {
    public int physicsSmoothingSteps = 5;

    private Vector3[] _previousPositions;

    private int _currentIndex;

    private void Start()
    {
      _previousPositions = new Vector3[physicsSmoothingSteps];
    }

    private void FixedUpdate()
    {
      var t = transform;
      _previousPositions[_currentIndex] = t.position;
      _currentIndex = (_currentIndex + 1) % _previousPositions.Length;
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      base.OnGrab(interactor, start);

      if (!start)
      {
        var t = transform;
        var velocity = (t.position - _previousPositions[_currentIndex]) /
                       (_previousPositions.Length * Time.fixedDeltaTime);
        rb.velocity = velocity;
      }
    }
  }
}
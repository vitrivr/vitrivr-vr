using TMPro;
using UnityEngine;
using VitrivrVR.Interaction.System;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Class representing a query term for query ordering.
  /// </summary>
  public class QueryTermProviderRepresentation : EventInteractable
  {
    public TMP_Text text;

    private QueryTermManager _termManager;

    public void Initialize(QueryTermManager termManager, QueryTermProvider provider, Vector3 offset)
    {
      text.text = provider.name;
      _offset = offset;
      _termManager = termManager;
    }

    public void UpdateOffset(float offsetX)
    {
      _offset.x = offsetX;
    }

    #region GrabBehaviour

    private bool _grabbed;
    private Vector3 _offset;
    private Transform _interactor;

    private void Update()
    {
      var t = transform;
      var otherT = _termManager.transform;

      var offset = _offset;

      if (_grabbed)
      {
        offset.x = otherT.InverseTransformPoint(_interactor.position).x;
      }

      // Reset position relative to combined query term provider
      t.position = otherT.TransformPoint(offset);
      t.rotation = otherT.rotation;
    }

    public override void OnGrab(Transform interactor, bool start)
    {
      base.OnGrab(interactor, start);

      _interactor = start ? interactor : null;
      _grabbed = start;

      if (start)
      {
        _termManager.StartReordering();
      }
      else
      {
        _termManager.EndReordering();
        _termManager.Reorganize(this, _termManager.transform.InverseTransformPoint(transform.position).x);
      }
    }

    #endregion
  }
}
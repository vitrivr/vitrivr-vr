using TMPro;
using UnityEngine;
using VitrivrVR.Interaction.System;
using VitrivrVR.Util;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Class representing a query term for query ordering.
  /// </summary>
  public class QueryTermProviderRepresentation : EventInteractable
  {
    public TMP_Text text;
    public ConnectionLineController connectionLine;

    public string TypeName { get; private set; }
    public int N { get; private set; }

    private QueryTermManager _termManager;

    /// <summary>
    /// Initializes this representation with a offset and name information.
    /// </summary>
    /// <param name="termManager">The associated term manager.</param>
    /// <param name="provider">The associated term provider.</param>
    /// <param name="offset">The position offset to the term manager in term manager coordinates.</param>
    /// <param name="typeName">The name of the type of term provider.</param>
    /// <param name="n">A number to differentiate this representation from others of the same term type.</param>
    public void Initialize(QueryTermManager termManager, QueryTermProvider provider, Vector3 offset, string typeName,
      int n)
    {
      TypeName = typeName;
      N = n;
      text.text = $"{typeName} {n}";
      _offset = offset;
      _termManager = termManager;
      connectionLine.end = provider.transform;
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
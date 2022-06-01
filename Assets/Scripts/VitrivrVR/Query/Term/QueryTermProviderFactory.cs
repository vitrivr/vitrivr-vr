using UnityEngine;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Instantiates query term providers.
  /// </summary>
  public class QueryTermProviderFactory : MonoBehaviour
  {
    public QueryTermManager combinedQueryTermProvider;
    public QueryTermProviderController queryTermProviderPrefab;

    private bool _grabbed;
    private Vector3 _offset;
    private Quaternion _rotationOffset;

    private void Start()
    {
      var transform1 = combinedQueryTermProvider.transform;
      _offset = transform1.InverseTransformPoint(transform.position);
      _rotationOffset = Quaternion.Inverse(transform1.rotation);
    }

    private void Update()
    {
      if (_grabbed)
      {
        return;
      }

      // Reset position relative to combined query term provider
      var t = transform;
      var otherT = combinedQueryTermProvider.transform;
      t.position = otherT.TransformPoint(_offset);
      t.rotation = otherT.rotation * _rotationOffset;
    }

    private void CreateQueryTermProvider()
    {
      var transform1 = transform;
      var providerController = Instantiate(queryTermProviderPrefab, transform1.position, transform1.rotation);
      combinedQueryTermProvider.Add(providerController.queryTermProvider);
      providerController.onClose = () =>
        combinedQueryTermProvider.Remove(providerController.queryTermProvider);
    }

    public void OnGrab()
    {
      _grabbed = true;
    }

    public void OnDrop()
    {
      _grabbed = false;
      CreateQueryTermProvider();
    }
  }
}
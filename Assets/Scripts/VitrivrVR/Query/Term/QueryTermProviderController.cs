using System;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Controller for Query Term Providers to act as interface to Query Term Provider Factories.
  /// </summary>
  public class QueryTermProviderController : MonoBehaviour
  {
    public QueryTermProvider queryTermProvider;
    public Action onClose;

    public void Close()
    {
      onClose();
      Cleanup();
    }

    /// <summary>
    /// Performs the actual closing operations.
    /// </summary>
    protected void Cleanup()
    {
      Destroy(gameObject);
    }
  }
}

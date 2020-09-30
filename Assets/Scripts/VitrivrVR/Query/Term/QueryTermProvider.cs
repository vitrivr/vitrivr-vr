using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;
using UnityEngine;

namespace VitrivrVR.Query.Term
{
  public abstract class QueryTermProvider : MonoBehaviour
  {
    public abstract List<QueryTerm> GetTerms();
  }
}
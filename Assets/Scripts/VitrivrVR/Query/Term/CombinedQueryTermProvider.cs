using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;

namespace VitrivrVR.Query.Term
{
  public class CombinedQueryTermProvider : QueryTermProvider
  {
    public List<QueryTermProvider> queryTermProviders;
    public override List<QueryTerm> GetTerms()
    {
      var terms = new List<QueryTerm>();
      foreach (var provider in queryTermProviders)
      {
        terms.AddRange(provider.GetTerms());
      }
      return terms;
    }

    public override string GetTypeName()
    {
      return "Combined";
    }
  }
}
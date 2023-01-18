using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;
using TMPro;

namespace VitrivrVR.Query.Term
{
  public class CombinedQueryTermProvider : QueryTermProvider
  {
    public List<QueryTermProvider> queryTermProviders;
    public TMP_Text nameDisplayText;
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
    
    public override void SetInstanceName(string displayName)
    {
      if (nameDisplayText != null)
      {
        nameDisplayText.text = displayName;
      }
    }
  }
}
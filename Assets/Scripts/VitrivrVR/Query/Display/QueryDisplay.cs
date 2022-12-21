using System.Linq;
using System.Text;
using Org.Vitrivr.CineastApi.Model;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Utils;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Abstract class for displaying queries.
  /// </summary>
  public abstract class QueryDisplay : MonoBehaviour
  {
    public virtual int NumberOfResults => -1;

    public QueryResponse QueryData { get; private set; }

    public void Initialize(QueryResponse queryResult)
    {
      QueryData = queryResult;
      Initialize();
    }

    /// <summary>
    /// Returns a string representation of the query that resulted in this query display.
    /// </summary>
    /// <returns>String representation of the associated query.</returns>
    public virtual string GetQueryStringRepresentation()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append("{");
      if (QueryData.Query != null)
      {
        stringBuilder.Append(string.Join(", ", QueryData.Query.Terms.Select(TermToString)));
      }
      else if (QueryData.StagedQuery != null)
      {
        stringBuilder.Append("{");
        stringBuilder.Append(string.Join("}, {",
          QueryData.StagedQuery.Stages.Select(stage => string.Join(", ", stage.Terms.Select(TermToString)))));
        stringBuilder.Append("}");
      }

      stringBuilder.Append("}");

      return stringBuilder.ToString();
    }

    protected abstract void Initialize();

    /// <summary>
    /// Turns the given <see cref="QueryTerm"/> into a string representation.
    /// </summary>
    protected static string TermToString(QueryTerm term)
    {
      var categories = string.Join(", ", term.Categories);
      var baseString = $"{term.Type} ({categories})";
      switch (term.Type)
      {
        case QueryTerm.TypeEnum.IMAGE:
          return baseString;
        case QueryTerm.TypeEnum.BOOLEAN:
        case QueryTerm.TypeEnum.TAG:
        {
          var data = Base64Converter.StringFromBase64(term.Data[Base64Converter.JsonPrefix.Length..]);
          return $"{baseString}: {data}";
        }
        default:
          return $"{baseString}: {term.Data}";
      }
    }
  }
}
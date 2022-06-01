using System.Linq;
using System.Text;
using Vitrivr.UnityInterface.CineastApi.Model.Data;

namespace VitrivrVR.Query.Display
{
  public abstract class TemporalQueryDisplay : QueryDisplay
  {
    public TemporalQueryResponse TemporalQueryData => temporalQueryData;

    protected TemporalQueryResponse temporalQueryData;

    public void Initialize(TemporalQueryResponse queryResponse)
    {
      temporalQueryData = queryResponse;
      Initialize();
    }

    public override string GetQueryStringRepresentation()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append("{");
      stringBuilder.Append("{");
      stringBuilder.Append(string.Join("}, {", temporalQueryData.Query.Queries.Select(
        temporal => string.Join("}, {", temporal.Stages.Select(
          stage => string.Join(", ", stage.Terms.Select(TermToString))
        ))
      )));
      stringBuilder.Append("}");

      stringBuilder.Append("}");

      return stringBuilder.ToString();
    }
  }
}
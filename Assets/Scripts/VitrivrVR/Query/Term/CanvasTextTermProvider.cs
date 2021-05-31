using System.Collections.Generic;
using Org.Vitrivr.CineastApi.Model;

namespace VitrivrVR.Query.Term
{
  public class CanvasTextTermProvider : QueryTermProvider
  {
    // Text input data
    // TODO: Restructure to be modular and configurable
    private bool _ocr;
    private bool _asr;
    private bool _sceneCaption;
    private bool _visualTextCoEmbedding;
    private string _textSearchText;

    public void SetTextSearchText(string text)
    {
      _textSearchText = text;
    }

    public void SetOcrSearch(bool ocr)
    {
      _ocr = ocr;
    }

    public void SetAsrSearch(bool asr)
    {
      _asr = asr;
    }

    public void SetSceneCaptionSearch(bool sceneCaption)
    {
      _sceneCaption = sceneCaption;
    }

    public void SetVisualTextCoEmbeddingSearch(bool visualTextCoEmbedding)
    {
      _visualTextCoEmbedding = visualTextCoEmbedding;
    }

    public override List<QueryTerm> GetTerms()
    {
      var terms = new List<QueryTerm>();
      if ((_ocr || _asr || _sceneCaption || _visualTextCoEmbedding) && !string.IsNullOrEmpty(_textSearchText))
      {
        terms.Add(BuildTextTerm());
      }

      return terms;
    }

    private QueryTerm BuildTextTerm()
    {
      // TODO: Move to Cineast Unity Interface in a more modular way
      var categories = new List<string>();

      if (_ocr)
      {
        categories.Add("ocr");
      }

      if (_asr)
      {
        categories.Add("asr");
      }

      if (_sceneCaption)
      {
        categories.Add("scenecaption");
      }

      if (_visualTextCoEmbedding)
      {
        categories.Add("visualtextcoembedding");
      }

      return new QueryTerm(QueryTerm.TypeEnum.TEXT, _textSearchText, categories);
    }
  }
}
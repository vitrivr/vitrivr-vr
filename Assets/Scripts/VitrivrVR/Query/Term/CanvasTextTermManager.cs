using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VitrivrVR.Query.Term
{
  /// <summary>
  /// Class to create and manage multiple text term providers.
  /// </summary>
  public class CanvasTextTermManager : QueryTermProvider
  {
    [Tooltip("The prefab from which to create text term providers.")]
    public CanvasTextTermProvider textTermProviderPrefab;

    private CanvasTextTermProvider _textTermProvider;
    private readonly List<CanvasTextTermProvider> _providers = new List<CanvasTextTermProvider>();

    private void Awake()
    {
      _textTermProvider = GetComponentInChildren<CanvasTextTermProvider>();
    }

    public override List<QueryTerm> GetTerms()
    {
      var terms = _providers.SelectMany(provider => provider.GetTerms()).ToList();
      terms.AddRange(_textTermProvider.GetTerms());
      return terms;
    }

    public override string GetTypeName()
    {
      return "Text (Multiple)";
    }

    public void AddTextTermProvider()
    {
      var provider = Instantiate(textTermProviderPrefab, transform);
      _providers.Add(provider);

      provider.transform.SetSiblingIndex(_providers.Count);

      // Rewire clear button
      var buttonTransform = provider.transform.GetChild(5);
      var removeButton = buttonTransform.GetComponent<Button>();
      removeButton.onClick.AddListener(() => Remove(provider));
      buttonTransform.GetComponentInChildren<TMP_Text>().text = "REMOVE";
    }

    public void Remove(CanvasTextTermProvider provider)
    {
      _providers.Remove(provider);
      Destroy(provider.gameObject);
    }
  }
}
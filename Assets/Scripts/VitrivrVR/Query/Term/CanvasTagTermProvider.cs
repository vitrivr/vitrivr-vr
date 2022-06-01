using System.Collections.Generic;
using System.Linq;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Utils;
using VitrivrVR.Util;

namespace VitrivrVR.Query.Term
{
  public class CanvasTagTermProvider : QueryTermProvider
  {
    public GameObject tagButtonPrefab;
    public GameObject tagItemPrefab;
    public RectTransform searchScrollViewContent;
    public RectTransform tagScrollViewContent;
    public RectTransform toolTipPanel;
    public TextMeshProUGUI toolTipText;
    public int maxResults = 100;

    private readonly List<TagData> _tagItems = new List<TagData>();
    private readonly HashSet<string> _tagIds = new HashSet<string>();

    /// <summary>
    /// Stores the latest tag search input to determine if search results are still relevant.
    /// </summary>
    private string _latestInput;
    
    /// <summary>
    /// Retrieves tags similar to the text input and adds corresponding buttons to the search scroll view.
    /// </summary>
    /// <param name="input">Text to use for tag search</param>
    public async void GetTags(string input)
    {
      _latestInput = input;
      if (input == "")
      {
        ClearTagButtons();
        return;
      }

      var tags = await CineastWrapper.GetMatchingTags(input);
      if (input != _latestInput)
      {
        // A tag search with a different input has been started and the results from this search are no longer relevant
        return;
      }

      ClearTagButtons();
      tags.Sort((t0, t1) => t0.Name.Length - t1.Name.Length);
      foreach (var resultTag in tags.Take(maxResults))
      {
        CreateNewTagButton(resultTag.Name, resultTag.Id, resultTag.Description);
      }
    }

    private void ClearTagButtons()
    {
      foreach (Transform child in searchScrollViewContent)
      {
        Destroy(child.gameObject);
      }
    }

    private void CreateNewTagButton(string tagName, string tagId, string tagDescription)
    {
      var buttonObject = Instantiate(tagButtonPrefab, searchScrollViewContent);
      // Set button text
      var textMesh = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
      textMesh.text = tagName;
      // Set button action
      var button = buttonObject.GetComponent<Button>();
      button.onClick.AddListener(() => CreateNewTagItem(tagName, tagId));
      // Set hover text
      var hoverHandler = buttonObject.AddComponent<HoverHandler>();
      var tooltip = tagDescription.Length == 0 ? "Description missing." : tagDescription;
      hoverHandler.onEnter = _ => SetTooltip(tooltip);
      hoverHandler.onExit = _ => DisableTooltip();
    }

    private void CreateNewTagItem(string tagName, string tagId)
    {
      if (_tagIds.Contains(tagId))
      {
        // Tag already in tag list, return
        return;
      }

      var item = Instantiate(tagItemPrefab, tagScrollViewContent);
      // Set item text
      var textMesh = item.GetComponentInChildren<TextMeshProUGUI>();
      textMesh.text = tagName;
      // Set tag data
      var tagData = item.GetComponent<TagData>();
      tagData.TagName = tagName;
      tagData.TagId = tagId;
      // Set button action
      var button = item.GetComponentInChildren<Button>();
      button.onClick.AddListener(() => RemoveTagItem(tagData));
      _tagItems.Add(tagData);
      _tagIds.Add(tagId);
    }

    private void RemoveTagItem(TagData tagData)
    {
      var index = _tagItems.IndexOf(tagData);
      _tagIds.Remove(tagData.TagId);
      _tagItems.RemoveAt(index);

      Destroy(tagData.gameObject);
    }

    private void SetTooltip(string tooltip)
    {
      toolTipText.text = tooltip;
      toolTipPanel.gameObject.SetActive(true);
    }

    private void DisableTooltip()
    {
      toolTipPanel.gameObject.SetActive(false);
    }

    public override List<QueryTerm> GetTerms()
    {
      var terms = new List<QueryTerm>();
      var tags = _tagItems.Select(tagItem => (tagItem.TagId, tagItem.TagName)).ToList();
      if (tags.Count > 0)
      {
        terms.Add(QueryTermBuilder.BuildTagTerm(tags));
      }

      return terms;
    }

    public override string GetTypeName()
    {
      return "Tag";
    }
  }
}
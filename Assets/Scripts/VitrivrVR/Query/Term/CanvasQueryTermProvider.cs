using System.Collections.Generic;
using System.Linq;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Model;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace VitrivrVR.Query.Term
{
  public class CanvasQueryTermProvider : QueryTermProvider
  {
    public GameObject tagButtonPrefab;
    public GameObject tagItemPrefab;
    public RectTransform searchScrollViewContent;
    public RectTransform tagScrollViewContent;
    public int maxResults = 100;

    private readonly List<TagData> _tagItems = new List<TagData>();
    private readonly HashSet<string> _tagIds = new HashSet<string>();

    // Text input data
    private bool _ocr;
    private bool _asr;
    private bool _sceneCaption;
    private string _textSearchText;

    // Stores the latest tag search input to determine if search results are still relevant
    private string _latestInput;
    private int _searchViewChildCount;
    private float _tagButtonHeight;
    private float _tagItemHeight;

    void Awake()
    {
      var tagButtonRect = tagButtonPrefab.GetComponent<RectTransform>();
      _tagButtonHeight = tagButtonRect.rect.height;
      var tagItemRect = tagItemPrefab.GetComponent<RectTransform>();
      _tagItemHeight = tagItemRect.rect.height;
    }

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
        CreateNewTagButton(resultTag.Name, resultTag.Id);
      }
    }

    void ClearTagButtons()
    {
      foreach (Transform child in searchScrollViewContent)
      {
        Destroy(child.gameObject);
      }

      _searchViewChildCount = 0;
      searchScrollViewContent.sizeDelta = new Vector2(0, 0);
    }

    void CreateNewTagButton(string tagName, string tagId)
    {
      searchScrollViewContent.sizeDelta = new Vector2(0, searchScrollViewContent.sizeDelta.y + _tagButtonHeight);
      GameObject buttonObject = Instantiate(tagButtonPrefab, searchScrollViewContent);
      // Set button position
      RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
      buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, -_tagButtonHeight *
        _searchViewChildCount - _tagButtonHeight / 2);
      // Set button text
      TextMeshProUGUI textMesh = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
      textMesh.text = tagName;
      // Set button action
      Button button = buttonObject.GetComponent<Button>();
      button.onClick.AddListener(() => CreateNewTagItem(tagName, tagId));
      _searchViewChildCount++;
    }

    void CreateNewTagItem(string tagName, string tagId)
    {
      if (_tagIds.Contains(tagId))
      {
        // Tag already in tag list, return
        return;
      }

      tagScrollViewContent.sizeDelta = new Vector2(0, tagScrollViewContent.sizeDelta.y + _tagItemHeight);
      GameObject item = Instantiate(tagItemPrefab, tagScrollViewContent);
      // Set item position
      RectTransform itemTransform = item.GetComponent<RectTransform>();
      itemTransform.anchoredPosition = new Vector2(itemTransform.anchoredPosition.x, -_tagItemHeight *
        _tagItems.Count);
      // Set item text
      TextMeshProUGUI textMesh = item.GetComponentInChildren<TextMeshProUGUI>();
      textMesh.text = tagName;
      // Set tag data
      TagData tagData = item.GetComponent<TagData>();
      tagData.TagName = tagName;
      tagData.TagId = tagId;
      // Set button action
      Button button = item.GetComponentInChildren<Button>();
      button.onClick.AddListener(() => RemoveTagItem(tagData));
      _tagItems.Add(tagData);
      _tagIds.Add(tagId);
    }

    void RemoveTagItem(TagData tagData)
    {
      int index = _tagItems.IndexOf(tagData);
      _tagIds.Remove(tagData.TagId);
      _tagItems.RemoveAt(index);
      foreach (var item in _tagItems.Skip(index))
      {
        RectTransform itemTransform = item.GetComponent<RectTransform>();
        var anchoredPosition = itemTransform.anchoredPosition;
        anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y + _tagItemHeight);
        itemTransform.anchoredPosition = anchoredPosition;
      }

      Destroy(tagData.gameObject);
    }

    public override List<QueryTerm> GetTerms()
    {
      var terms = new List<QueryTerm>();
      var tags = _tagItems.Select(tagItem => (tagItem.TagId, tagItem.TagName)).ToList();
      if (tags.Count > 0)
      {
        terms.Add(QueryTermBuilder.BuildTagTerm(tags));
      }

      if ((_ocr || _asr || _sceneCaption) && !string.IsNullOrEmpty(_textSearchText))
      {
        terms.Add(QueryTermBuilder.BuildTextTerm(_textSearchText, _ocr, _asr, _sceneCaption));
      }

      return terms;
    }
  }
}
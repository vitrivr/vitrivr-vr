using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using Org.Vitrivr.CineastApi.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class TagInputController : MonoBehaviour
{
    public GameObject tagButtonPrefab;
    public GameObject tagItemPrefab;
    public RectTransform searchScrollViewContent;
    public RectTransform tagScrollViewContent;
    public TMP_InputField tagTextField;
    public int maxResults = 50;

    public readonly List<TagData> TagItems = new List<TagData>();
    
    private TagApi _tagApi;

    // Stores the latest tag search input to determine if search results are still relevant
    private string _latestInput;
    private int _searchViewChildCount;
    private float _tagButtonHeight;
    private float _tagItemHeight;

    void Awake()
    {
        _tagApi = new TagApi(CineastConfigManager.Instance.ApiConfiguration);
        var tagButtonRect = tagButtonPrefab.GetComponent<RectTransform>();
        _tagButtonHeight = tagButtonRect.rect.height;
        var tagItemRect = tagItemPrefab.GetComponent<RectTransform>();
        _tagItemHeight = tagItemRect.rect.height;
    }

    public async void GetTags(string input)
    {
        _latestInput = input;
        if (input == "")
        {
            ClearTagButtons();
            return;
        }

        var tagsQueryResult = await Task.Run(() => _tagApi.FindTagsBy("matchingname", input));
        if (input != _latestInput)
        {
            // A tag search with a different input has been started and the results from this search are no longer relevant
            return;
        }

        ClearTagButtons();
        var tags = tagsQueryResult.Tags;
        tags.Sort((t0, t1) => t0.Name.Length - t1.Name.Length);
        foreach (var resultTag in tagsQueryResult.Tags.Take(maxResults))
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
        tagScrollViewContent.sizeDelta = new Vector2(0, tagScrollViewContent.sizeDelta.y + _tagItemHeight);
        GameObject item = Instantiate(tagItemPrefab, tagScrollViewContent);
        // Set item position
        RectTransform itemTransform = item.GetComponent<RectTransform>();
        itemTransform.anchoredPosition = new Vector2(itemTransform.anchoredPosition.x, -_tagItemHeight *
            TagItems.Count);
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
        TagItems.Add(tagData);
    }

    void RemoveTagItem(TagData tagData)
    {
        int index = TagItems.IndexOf(tagData);
        TagItems.RemoveAt(index);
        foreach (var item in TagItems.Skip(index))
        {
            RectTransform itemTransform = item.GetComponent<RectTransform>();
            var anchoredPosition = itemTransform.anchoredPosition;
            anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y + _tagItemHeight);
            itemTransform.anchoredPosition = anchoredPosition;
        }

        Destroy(tagData.gameObject);
    }
}
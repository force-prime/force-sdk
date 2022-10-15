using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject nftItemPrefab;
    [SerializeField] private Button closeButton;

    private readonly List<HighScoreItemUI> _items = new List<HighScoreItemUI>();
    private readonly List<GameObject> _nftItems = new List<GameObject>();

    static public bool IsVisible = false;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        IsVisible = false;
    }

    private void Update()
    {
        var wasVisible = transform.GetChild(0).gameObject.activeSelf;
        transform.GetChild(0).gameObject.SetActive(IsVisible);

        if (IsVisible && !wasVisible) 
        {
            UpdateScoreItems();
            UpdateMemorableNFTs();
        }
    }

    private void UpdateScoreItems()
    {
        var scores = HighScores.Load();

        var panel = transform.GetChild(0);
        var originalPos = itemPrefab.GetComponent<RectTransform>().anchoredPosition;

        for (int i = _items.Count; i < scores.Count; i++)
        {
            var item = Instantiate(itemPrefab, panel);
            _items.Add(item.GetComponent<HighScoreItemUI>());
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(originalPos.x, -30 + (i + 1) * -50);
        }

        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].gameObject.SetActive(i < scores.Count);
            if (_items[i].gameObject.activeSelf)
                _items[i].Attach(i + 1, scores[i].name, scores[i].score);
        }
    }
    
    private async void UpdateMemorableNFTs()
    {
        var stream = Game.Current.Wallet.GetNFTs(MintNFT.NFT_ID, false);
        var nfts = await stream.ReadMoreAsync(10);

        var panel = transform.GetChild(0);

        var originalPos = nftItemPrefab.GetComponent<RectTransform>().anchoredPosition;

        for (int i = _nftItems.Count; i < nfts.Count; i++)
        {
            var item = Instantiate(nftItemPrefab, panel);
            _nftItems.Add(item);
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(90 + i * 40, originalPos.y);
        }

        for (int i = 0; i < _nftItems.Count; i++)
        {
            _nftItems[i].gameObject.SetActive(i < nfts.Count);
            if (_nftItems[i].gameObject.activeSelf)
            {
                var value = nfts[i].GetNFTId();
                var score = ((value as Clarity.Tuple).Values["score"] as Clarity.UInteger128).Value;
                _nftItems[i].GetComponentInChildren<TMP_Text>().text = score.ToString();
            }
        }
    }
}

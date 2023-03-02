using ChainAbstractions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectNFTUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text size;
    [SerializeField] private TMP_Text gravity;
    [SerializeField] private TMP_Text strength;
    [SerializeField] private TMP_Text luck;
    [SerializeField] private TMP_Dropdown dropdown;

    private readonly List<INFT> _nfts = new List<INFT>();
    private bool _nftRequested = false;
    private bool _loaded = false;

    void Awake()
    {
        selectButton.onClick.AddListener(OnSelectClick);
        dropdown.onValueChanged.AddListener(OnNFTChanged);
    }

    private void Update()
    {
        var inSelection = Game.Current.CurrentState == Game.State.Selecting;
        if (inSelection && !_nftRequested)
        {
            _nftRequested = true;
            FillNFTs();
        }

        transform.GetChild(0).gameObject.SetActive(inSelection);
    }

    private async void FillNFTs()
    {
        Debug.Log("FillNFTs");
        if (Game.Current.Wallet == null)
            return;
        
        var stream = Game.Current.Wallet.GetNFTs(null, false);

        var nfts = await stream.ReadMoreAsync(50);
        
        Debug.Log($"Added {nfts.Count} NFTs");
        dropdown.AddOptions(nfts.Select(x => x.Name).ToList());

        if (_nfts.Count == 0 && nfts.Count > 0) // select first if none selected
        {
            SetSelected(nfts[0]);
        }

        if (nfts.Count == 0)
        {
            dropdown.placeholder.GetComponent<TMP_Text>().text = "No nfts...";
            SetSelected(null);
        }

        _loaded = true;
        _nfts.AddRange(nfts);
    }

    private void PrintStats(Flappy player)
    {
        speed.text = "Speed: " + player.speed.ToString("0.00");
        size.text = "Size: " + player.size.ToString("0.00");
        strength.text = "Str: " + player.strength.ToString("0.00");
        luck.text = "Luck: " + player.luck.ToString("0.00");
        gravity.text = "Gravity: " + player.gravity.ToString("0.00");
    }

    private void OnNFTChanged(int index)
    {
        SetSelected(_nfts[index]);
    }

    private void SetSelected(INFT nft)
    {
        Game.Current.AssignNft(nft);
        PrintStats(Game.Current.Player);

        if (nft != null)
            NftMeta.GetNft(nft);
    }

    private void OnSelectClick()
    {
        if (_loaded)
            Game.Current.CompleteSelection();
    }
}

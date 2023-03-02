using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text distance;
    [SerializeField] private TMP_Text startLabel;
    [SerializeField] private TMP_Text nftName;
    [SerializeField] private Image nftImage;
    [SerializeField] private Button highscoreButton;

    private string _currentNftUrl;
    private string _lastRequestNftUrl;

    private void Awake()
    {
        highscoreButton.onClick.AddListener(OnHighscoreButton);
    }

    void Update()
    {
        startLabel.gameObject.SetActive(Game.Current.CurrentState == Game.State.Selected);
        distance.text = Game.Current.Distance.ToString("0.00");
        nftName.text = Game.Current.NFT != null ? Game.Current.NFT.Name : string.Empty;
        UpdateNftImage();
    }

    private async void UpdateNftImage()
    {
        _currentNftUrl = Game.Current.NFT != null ? Game.Current.NFT.ImageUrl : null;

        if (!string.IsNullOrEmpty(_currentNftUrl))
        {
            if (_lastRequestNftUrl == _currentNftUrl)
                return;

            _lastRequestNftUrl = _currentNftUrl;
            var sprite = await NftMeta.GetImage(_lastRequestNftUrl);

            if (_lastRequestNftUrl == _currentNftUrl)
            {
                nftImage.sprite = sprite;
                nftImage.gameObject.SetActive(sprite != null);
            }
        } else
        {
            nftImage.gameObject.SetActive(false);
        }
    }

    private void OnHighscoreButton()
    {
        HighScoreUI.IsVisible = true;
    }
}

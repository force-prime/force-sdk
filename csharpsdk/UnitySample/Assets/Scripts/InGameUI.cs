using StacksForce;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text distance;
    [SerializeField] private TMP_Text startLabel;
    [SerializeField] private TMP_Text nftName;
    [SerializeField] private NftSpriteProvider nftImage;
    [SerializeField] private Button highscoreButton;

    private void Awake()
    {
        highscoreButton.onClick.AddListener(OnHighscoreButton);
    }

    void Update()
    {
        startLabel.gameObject.SetActive(Game.Current.CurrentState == Game.State.Selected);
        distance.text = Game.Current.Distance.ToString("0.00");
        nftName.text = Game.Current.NFT != null ? Game.Current.NFT.Name : string.Empty;
        nftImage.NFT = Game.Current.NFT;
    }

    private void OnHighscoreButton()
    {
        HighScoreUI.IsVisible = true;
    }
}

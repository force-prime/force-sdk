using ChainAbstractions;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CompletedUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button newNftButton;
    [SerializeField] private Button createNftButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(OnRestartClick);
        newNftButton.onClick.AddListener(OnNewNftClick);
        createNftButton.onClick.AddListener(OnCreateNftClick);
    }

    private async void OnCreateNftClick()
    {
        if (Game.Current.Wallet is IBasicWallet w)
        {
            var t = await MintNFT.GetMintTransaction(w, 29);
            SendTransactionUI.Show(t);
        } else
        {
            PortalJS.SendNftMintRequest((int)Math.Floor(Game.Current.Distance), GameLoader.Token);
        }
    }

    private void SendTransaction()
    {

    }

    private void OnNewNftClick()
    {
        Game.Current.Restart(true);
    }

    private void OnRestartClick()
    {
        Game.Current.Restart(false);
    }

    private void Update()
    {
        transform.GetChild(0).gameObject.SetActive(Game.Current.CurrentState == Game.State.Completed);
    }
}

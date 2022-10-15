using ChainAbstractions;
using ChainAbstractions.Stacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button button;

    private IBlockchain _chain;

    void Awake()
    {
#if DEBUG
        _chain = StacksAbstractions.TestNet;
#else
        _chain = StacksAbstractions.MainNet;
#endif
    }

    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        var mnemonic = input.text;
        var wallet = _chain.GetWalletForMnemonic(mnemonic);
        if (wallet != null)
        {
            Debug.Log("Wallet initialized: " + wallet.GetAddress());
            Game.Current.Login(wallet);
        }
        else
        {
            // handle incorrect wallet input
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).gameObject.SetActive(Game.Current.CurrentState == Game.State.Login);
    }
}

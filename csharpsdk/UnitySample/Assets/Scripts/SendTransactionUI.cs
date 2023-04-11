using ChainAbstractions;
using ChainAbstractions.Stacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendTransactionUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text costText;

    private ITransaction _transaction;

    private bool _isVisible;
    static private SendTransactionUI _current;

    private void Awake()
    {
        _current = this;

        sendButton.onClick.AddListener(OnSendClick);
        closeButton.onClick.AddListener(OnCloseClick);
    }

    private void Update()
    {
        transform.GetChild(0).gameObject.SetActive(_isVisible);
    }

    private void OnCloseClick()
    {
        _isVisible = false;
    }

    private async void OnSendClick()
    {
        var result = await _transaction.Send();
        if (result != null)
            costText.text = result.ToString();
        else
            costText.text = "Broadcasted succesfully!";
    }

    static public void Show(ITransaction transaction)
    {
        _current.AttachTransaction(transaction);
    }

    public void AttachTransaction(ITransaction transaction)
    {
        _isVisible = true;

        _transaction = transaction;

        costText.text = "Transaction cost: " + _transaction.Cost.BalanceFormatted();
    }
}

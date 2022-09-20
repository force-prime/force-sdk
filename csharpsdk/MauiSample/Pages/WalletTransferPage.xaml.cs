using ChainAbstractions.Stacks;
using MauiSample.Pages.Popups;
using MauiSample.ViewModels;

namespace MauiSample.Pages;

public partial class WalletTransferPage : ContentPage
{
	private WalletViewModel _vm;

	public WalletTransferPage()
	{
		InitializeComponent();
		BindingContext = _vm = WalletPage.WalletVM;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_vm.GetCurrentTransactions();
	}

	private async void SendButtonClicked(object sender, EventArgs e)
	{
		if (!float.TryParse(amountEntry.Text, out var amount))
		{
			this.DisplayAlert("Error", "Incorrect amount", "OK");
			return;
		}

		var recepient = recepientEntry.Text;
        var transaction = await LoadingPopup.ShowWhile(this, _vm.GetTransferStx(recepient, amount, "testmemo"));
        if (transaction.Error != null)
            this.DisplayAlert("Error", transaction.Error.ToString(), "OK");
		else
		{
            var confirm = await this.DisplayAlert("Confirm", $"Send {StacksAbstractions.Stx.From(amount).BalanceFormatted()} to {recepient}, transaction cost = {transaction.Cost.BalanceFormatted()}", "Send", "Cancel");
			if (confirm)
			{
				var sendResult = await _vm.SendTransaction(transaction);
				if (sendResult != null)
                    this.DisplayAlert("Error", sendResult.ToString(), "OK");
				else
                    await _vm.GetCurrentTransactions();
            }
        }
        
    }

	private async void RequestButtonClicked(object sender, EventArgs e)
	{
		var result = await LoadingPopup.ShowWhile(this, _vm.RequestTestStx());
		if (result.Error != null)
			this.DisplayAlert("Error", result.Error.ToString(), "OK");
		else
		{
			await _vm.GetCurrentTransactions();
		}
    }

	private async void RefreshView_Refreshing(object sender, EventArgs e)
	{
		await _vm.GetCurrentTransactions();
		(sender as RefreshView).IsRefreshing = false;
	}

	private void OnItemTapped(object sender, EventArgs e)
	{
        var id = ((sender as Element).BindingContext as WalletViewModel.TransactionVM).Id;
        var url = $"https://explorer.stacks.co/txid/{id}?chain=testnet";
        Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }
}
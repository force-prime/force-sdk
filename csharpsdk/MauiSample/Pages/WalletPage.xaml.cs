using CommunityToolkit.Maui.Views;
using MauiSample.Pages.Popups;
using MauiSample.ViewModels;

namespace MauiSample.Pages;

public partial class WalletPage : ContentPage
{
	static public readonly WalletViewModel WalletVM = new WalletViewModel();

	public WalletPage()
	{
		InitializeComponent();
		BindingContext = WalletVM;

    }

	private async void NewWalletClicked(object sender, EventArgs e)
	{
		await LoadingPopup.ShowWhile(this, WalletVM.CreateWallet());
	}

	private async void RestoreWalletClicked(object sender, EventArgs e)
	{
		var restored = await LoadingPopup.ShowWhile(this, WalletVM.RestoreWallet(mnemonicEditor.Text));

		if (!restored)
			this.DisplayAlert("Error", "Incorrect mnemonic phrase", "OK");
	}
}
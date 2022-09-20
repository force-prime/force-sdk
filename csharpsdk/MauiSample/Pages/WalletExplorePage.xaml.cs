using ChainAbstractions.Stacks;
using MauiSample.ViewModels;
using MauiSample.Pages.Popups;

namespace MauiSample.Pages;

public partial class WalletExplorePage : ContentPage
{
    private WalletContentViewModel _vm;

	public WalletExplorePage()
	{
		InitializeComponent();
        BindingContext = _vm = new WalletContentViewModel();
	}

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        itemsCollection.HeightRequest = height - 130;
    }

    private async void ExploreButtonClicked(object sender, EventArgs e)
	{
        var walletInfo = StacksAbstractions.MainNet.GetWalletInfoForAddress(addressEntry.Text);
        if (walletInfo == null)
        {
            this.DisplayAlert("Error", "Incorrect address", "OK");
            return;
        }

        await LoadingPopup.ShowWhile(this, _vm.AssignWallet(walletInfo));
    }
}
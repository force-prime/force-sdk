using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MauiSample.ViewModels
{
    public class WalletContentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRefreshing { get; set; }

        public string Footer { get; set; }

        public ICommand LoadMoreCommand => new Command(LoadMoreData);

        public ICommand RefreshCommand => new Command(async () => await RefreshDataAsync());

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        private IDataStream<INFT> _nftStream;

        private IWalletInfo _wallet;
        private bool _isReading;

        public async Task AssignWallet(IWalletInfo wallet)
        {
            _wallet = wallet;
            await RefreshDataAsync();
        }

        private async Task PopulateWithFTs()
        {
            var fts = await _wallet.GetAllTokens();
            foreach (var v in fts)
            {
                Items.Add(new Item { Text = v.BalanceFormatted(), Description = v.Data.Description, Image = GetImageSourceForUrl(v.Data.ImageUrl, v.Data.Code) });
            }
        }

        private async Task<int> PopulateWithNFTs()
        {
            _isReading = true;
            Footer = "Loading...";

            if (_nftStream == null)
                _nftStream = _wallet.GetNFTs(null);

            var nfts = await _nftStream.ReadMoreAsync(2);

            foreach (var x in nfts)
            {
                var text = "NFT: " + x.Name;
                bool contains = false;
                foreach (var item in Items)
                    if (item.Text == text)
                    {
                        contains = true;
                        break;
                    }

                if (contains)
                    continue;

                Items.Add(new Item
                {
                    Text = text,
                    Description = x.Description,
                    Image = GetImageSourceForUrl(x.ImageUrl, x.Name)
                });
            }

            _isReading = false;
            Footer = null;
            IsRefreshing = false;
            return nfts.Count;
        }

        private async Task RefreshDataAsync()
        {
            Items.Clear();
            _nftStream = null;
            await PopulateWithFTs();
            await PopulateWithNFTs();
        }

        private void LoadMoreData(object obj)
        {
            if (IsRefreshing || _isReading)
                return;
            
            PopulateWithNFTs();
        }

        static private string GetImageSourceForUrl(string url, string code)
        {
            if (string.IsNullOrEmpty(url) || url.EndsWith("svg")) // workaround for displaying svg 
                url = $"https://ui-avatars.com/api/?name={code.Substring(0, 1)}&rounded=true&bold=true&background=60fc96";

            var imageUrl = HttpHelper.GetHttpUrlFrom(url);
            var uri = new System.Uri(imageUrl);
            return uri.ToString();
        }

        public class Item
        {
            public string Text { get; set; }
            public string Description { get; set; }
            public ImageSource Image { get; set; }
        }
    }
}

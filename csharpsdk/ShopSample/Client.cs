using ChainAbstractions;

namespace ShopSample
{
    public class Client
    {
        private readonly Shop _shop;
        private readonly IBasicWallet _clientWallet;

        private readonly string _nft;

        public string Address => _clientWallet.GetAddress();

        public Client(IBlockchain chain, string clientMnemonic, string contractAddress, string fullTokenId, string fullNftId)
        {
            _clientWallet = chain.GetWalletForMnemonic(clientMnemonic);
            _shop = new Shop(contractAddress, fullTokenId);
            _nft = fullNftId;
        }

        public async Task BuyNFT(ulong nftType, ulong maxTokenCost)
        {
            var t = await _shop.BuyNftForTokens(_clientWallet, nftType, maxTokenCost);
            await Utils.SendAndWait("buy nft", t);
        }

        public async Task PrintOwnedNFTs()
        {
            var nftStream = _clientWallet.GetNFTs(_nft, false);

            while (true)
            {
                var nfts = await nftStream.ReadMoreAsync(5);
                if (nfts == null)
                {
                    Console.WriteLine("ERROR reading nfts");
                    break;
                }

                foreach (var nft in nfts)
                    Console.WriteLine($"{nft.Name}");

                if (nfts.Count < 5)
                    break;
            }
        }
    }
}

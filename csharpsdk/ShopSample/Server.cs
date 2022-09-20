using ChainAbstractions;
using ChainAbstractions.Stacks;

namespace ShopSample
{
    public class Server
    {
        private readonly Shop _shop;
        private readonly IBasicWallet _adminWallet;
        private readonly string _token;
        private readonly string _nft;

        public string Address => _adminWallet.GetAddress();
        public string FullTokenId => _token;
        public string FullNFTId => _nft;

        public Server(IBlockchain chain, string serverWalletMnemonic)
        {
            _adminWallet = chain.GetWalletForMnemonic(serverWalletMnemonic);
            var address = _adminWallet.GetAddress();
            _token = $"{address}.basic-token::GAME-CURRENCY";
            _nft = $"{address}.basic-nfts::GAME-NFT";
            _shop = new Shop(address, _token);
        }

        public async Task Configure()
        {
            var manager = _adminWallet.GetTransactionManager();
            var address = _adminWallet.GetAddress();

            ITransaction t;

            // allow .shop contract to perform admin actions 
            t = await _shop.SetAdmins(manager, new string[] { address, $"{address}.shop" });
            await Utils.SendAndWait("set admins", t);

            // set two NFT offers: first for STX, second for your own token
            t = await _shop.SetNftOffers(manager, new (int type, int stxPrice, int tokenPrice)[] { (1, 2000000, 0), (2, 0, 100) });
            await Utils.SendAndWait("set offers", t);
        }

        public async Task GiveTokensTo(string recepient, ulong amount) {
            var manager = _adminWallet.GetTransactionManager();

            var t = await _shop.MintToken(manager, recepient, amount);
            await Utils.SendAndWait("mint tokens", t);
        }

    }
}

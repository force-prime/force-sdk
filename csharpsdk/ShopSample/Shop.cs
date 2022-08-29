using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;

namespace ShopSample
{
    public class Shop
    {
        private readonly string _address;
        private readonly string _fullTokenId;

        public Shop(string address, string fullTokenId)
        {
            _address = address;
            _fullTokenId = fullTokenId; 
        }

        public async Task<ITransaction> SetNftOffers(TransactionsManager m, (int type, int stxPrice, int tokenPrice)[] offers)
        {
            var offerList = new Clarity.List(offers.Select(x =>
            new Clarity.Tuple(new Dictionary<string, Clarity.Value>() {
                    { "type", new Clarity.UInteger128((ulong)x.type) },
                    { "token-price", new Clarity.UInteger128((ulong)x.tokenPrice) },
                    { "stx-price", new Clarity.UInteger128((ulong)x.stxPrice) }})).ToArray());

            var result = await m.ContractCall(_address, "shop", "update-nft-offers", offerList).ConfigureAwait(false);

            return new StacksAbstractions.TransactionInfoWrapper(result.Data, result.Error);
        }

        public async Task<ITransaction> SetAdmins(TransactionsManager m, string[] admins)
        {
            var result = await m.ContractCall(_address, "auth", "set-admins",
                    new Clarity.List(admins.Select(x => Clarity.Principal.FromString(x)).ToArray())
                ).ConfigureAwait(false);

            return new StacksAbstractions.TransactionInfoWrapper(result.Data, result.Error);
        }

        public async Task<ITransaction> Mint(TransactionsManager m, string recepient, ulong nftType)
        {
            var result = await m.ContractCall(_address, "basic-nft", "mint",
                    Clarity.Principal.FromString(recepient),
                    new Clarity.UInteger128(nftType)
                ).ConfigureAwait(false);

            return new StacksAbstractions.TransactionInfoWrapper(result.Data, result.Error);
        }

        public async Task<ITransaction> BuyNftForTokens(IBasicWallet caller, ulong nftType, ulong tokenCost)
        {
            var result = await caller.GetTransactionManager().ContractCall(_address, "shop", "buy-nft-tokens",
                new Clarity.Value[] {
                    new Clarity.UInteger128(nftType),
                    new Clarity.UInteger128(tokenCost)
                },
                new PostCondition[]
                {
                    new FungibleTokenPostCondition(caller.GetAddress(), new AssetInfo(_fullTokenId), FungibleConditionCode.LessEqual, tokenCost),
                }).ConfigureAwait(false);

            return new StacksAbstractions.TransactionInfoWrapper(result.Data, result.Error);
        }

    }
}

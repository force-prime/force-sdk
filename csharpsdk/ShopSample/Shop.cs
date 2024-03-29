﻿using ChainAbstractions;
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

            var result = await m.GetContractCall(_address, "shop", "update-nft-offers", 
                new Clarity.Value[] { offerList }).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(m, result);
        }

        public async Task<ITransaction> SetAdmins(TransactionsManager m, string[] admins)
        {
            var result = await m.GetContractCall(_address, "auth", "set-admins",
                    new Clarity.Value[] {
                        new Clarity.List(admins.Select(x => Clarity.Principal.FromString(x)).ToArray())
                    }
                ).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(m, result);
        }

        public async Task<ITransaction> MintNFT(TransactionsManager m, string recepient, ulong nftType)
        {
            var result = await m.GetContractCall(_address, "basic-nft", "mint",
                    new Clarity.Value[] {
                        Clarity.Principal.FromString(recepient),
                        new Clarity.UInteger128(nftType)
                    }
                ).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(m, result);
        }

        public async Task<ITransaction> MintToken(TransactionsManager m, string recepient, ulong amount)
        {
            var result = await m.GetContractCall(_address, "basic-token", "mint",
                    new Clarity.Value[] {
                        new Clarity.UInteger128(amount),
                        Clarity.Principal.FromString(recepient),
                    }
                ).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(m, result);
        }

        public async Task<ITransaction> BuyNftForTokens(IBasicWallet caller, ulong nftType, ulong tokenCost)
        {
            var manager = caller.GetTransactionManager();
            var result = await manager.GetContractCall(_address, "shop", "buy-nft-tokens",
                new Clarity.Value[] {
                    new Clarity.UInteger128(nftType),
                    new Clarity.UInteger128(tokenCost)
                },
                new PostCondition[]
                {
                    new FungibleTokenPostCondition(caller.GetAddress(), new AssetInfo(_fullTokenId), FungibleConditionCode.LessEqual, tokenCost),
                }).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(manager, result);
        }

    }
}

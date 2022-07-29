using StacksForce.Abstractions;
using StacksForce.Stacks.Metadata;
using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks
{
    static public class StacksAbstractions
    {
        static public string BalanceFormatted(this IFungibleToken token)
        {
            return token.FormatCount(token.Balance);
        }

        static public string FormatBalance(ulong count, uint decimals, string code)
        {
            if (decimals == 0)
                return count.ToString() + " " + code;

            var lowPartSize = (ulong)Math.Pow(10, decimals);
            var mainPart = count / lowPartSize;
            var fracPart = count % lowPartSize;
            return mainPart + "." + fracPart + " " + code;
        }

        static StacksAbstractions()
        {
            StacksDependencies.SetupDefault();
        }

        static public IBlockchain MainNet = new MainNetImpl();
        static public IBlockchain TestNet = new TestNetImpl();
        static public IFungibleTokenData Stx = new StxTokenData();

        private class StxTokenData : IFungibleTokenData
        {
            public const string STX_CODE = "STX";
            public const uint DECIMALS = 6;

            public string Code => STX_CODE;

            public string Description => "Stacks blockchain currency";

            public string ImageUrl => "https://assets-global.website-files.com/618b0aafa4afde65f2fe38fe/618b0aafa4afde785dfe397d_icon-stacks-gradient.svg";

            public string FormatCount(ulong count) => FormatBalance(count, DECIMALS, Code);
        }

        private class Transaction : ITransaction
        {
            public TransactionState State { get; private set; }
        }

        private class FungibleToken : IFungibleToken
        {
            public string Code { get; }
            public string Description { get; }
            public string ImageUrl { get; }

            public ulong Balance { get; }

            public uint Decimals { get; }

            public FungibleToken(ulong balance, string code, string description, string imageUrl, uint decimals)
            {
                Balance = balance;
                Code = code;
                Description = description;
                ImageUrl = imageUrl;
                Decimals = decimals;
            }

            public string FormatCount(ulong count) => FormatBalance(count, Decimals, Code);
        }

        private class NFT : INFT
        {
            public string Description { get; }
            public string ImageUrl { get; }
            public string Name { get; }

            public NFT(string name, string description, string imageUrl)
            {
                Name = name;
                Description = description;
                ImageUrl = imageUrl;
            }
        }

        private class BlockchainImplBasic : IBlockchain
        {
            private readonly Blockchain _blockchain;

            public BlockchainImplBasic(Blockchain blockchain)
            {
                this._blockchain = blockchain;
            }

            public IBasicWallet CreateNewWallet()
            {
                return BasicWalletImpl.CreateNew(_blockchain);
            }

            public IBasicWalletInfo? GetWalletInfoForAddress(string address)
            {
                var a = Address.FromC32(address);
                if (a == null)
                    return null;
                return new BasicWalletInfo(_blockchain, address);
            }
        }

        private class TestNetImpl : BlockchainImplBasic
        {
            public TestNetImpl() : base(Blockchains.Testnet) { }
        }

        private class MainNetImpl : BlockchainImplBasic
        {
            public MainNetImpl() : base(Blockchains.Mainnet) { }
        }

        private class BasicWalletImpl : BasicWalletInfo, IBasicWallet
        {
            private readonly StacksWallet _wallet;

            static public BasicWalletImpl FromMnemonic(Blockchain chain, string mnemonic)
            {
                var wallet = new StacksWallet(mnemonic);
                return new BasicWalletImpl(chain, new StacksWallet(mnemonic));
            }

            static public IBasicWallet CreateNew(Blockchain chain)
            {
                return FromMnemonic(chain, StacksWallet.GenerateMnemonicPhrase());
            }

            public BasicWalletImpl(Blockchain chain, StacksWallet wallet) : base (chain, wallet.GetAccount(0).GetAddress(chain.GetAddressVersion()))
            {
                _wallet = wallet;
            }

            public string GetMnemonic() => _wallet.Mnemonic;

            public Task<bool> PerformPurchase(string purchaseId)
            {
                throw new NotImplementedException();
            }
        }

        private class BasicWalletInfo : IBasicWalletInfo
        {
            private Blockchain _chain;
            private string _address;

            public event Action OnBalanceChanged;

            public BasicWalletInfo(Blockchain chain, string address)
            {
                _chain = chain;
                _address = address;
            }

            public async Task<IFungibleToken> GetToken(string currency)
            {
                if (string.IsNullOrEmpty(currency))
                    currency = Stx.Code;

                var result = await _chain.GetBalances(_address);
                if (result.IsSuccess)
                {
                    if (currency == Stx.Code)
                        return new FungibleToken(result.Data.stx.balance, Stx.Code, Stx.Description, Stx.ImageUrl, StxTokenData.DECIMALS);

                    if (result.Data.fungible_tokens.TryGetValue(currency, out var info))
                    {
                        return null; // TODO
                        //return info.balance;
                    }
                } 
                
                return null;
            }

            public IDataStream<INFT> GetNFTs(string nftType = null)
            {
                return new NFTStream(_chain, _address, nftType);
            }

            public async Task<List<IFungibleToken>> GetAllTokens()
            {
                var fts = new List<IFungibleToken>();
                var result = await _chain.GetBalances(_address);
                if (result.IsSuccess)
                {
                    foreach (var ft in result.Data.fungible_tokens)
                    {
                        var tokenAddressData = Address.ParseFromFullTokenId(ft.Key);
                        var metaDataResult = await FungibleTokenMetaData.ForTokenContract(_chain, tokenAddressData.address + "." + tokenAddressData.contract);
                        string currency = metaDataResult.Currency;
                        if (string.IsNullOrEmpty(currency))
                            currency = tokenAddressData.token;
                        string description = metaDataResult.Description;
                        if (string.IsNullOrEmpty(description))
                            description = metaDataResult.Name;
                        fts.Add(new FungibleToken(ft.Value.balance, currency, description, metaDataResult.Image, metaDataResult.Decimals));
                    }
                }
                return fts;
            }

            public string GetAddress() => _address;
        }
   
        private class NFTStream : BasicDataStream<INFT>
        {
            private Blockchain _chain;
            private string _address;
            private string _nftType;

            public NFTStream(Blockchain chain, string address, string nftType)
            {
                _chain = chain;
                _address = address;
                _nftType = nftType;
            }

            protected override async Task<List<INFT>> GetRange(long index, long count)
            {
                var nfts = new List<INFT>();
                var result = await _chain.GetNFTHoldings(_address, new string[] { _nftType }, false, (ulong) count, (ulong) index);
                if (result.IsSuccess)
                {
                    foreach (var t in result.Data.results)
                    {
                        var data = t.Extract();
                        if (data.id == null)
                            continue; // TODO: implement all types

                        var uri = await _chain.CallReadOnly(data.address, data.contract, "get-token-uri", _address, data.id);
                        if (uri.IsSuccess)
                        {
                            var uriStr = uri.Data.UnwrapUntil<Clarity.StringType>();
                            var metaData = await NFTMetaData.FromUrl(uriStr.Value);
                            var description = metaData.Description;
                            if (string.IsNullOrEmpty(description))
                                description = data.nft;

                            nfts.Add(new NFT(metaData.Name, description, metaData.Image));
                        } else
                        {
                            nfts.Add(new NFT(data.nft, "", String.Empty));
                        }
                    }
                }
                return nfts;
            }
        }

        /*
        private class MetaDataCache
        {
            private readonly CachedDictionaryAsync<string, NFTMetaData> _nftType2Data = new CachedDictionaryAsync<string, NFTMetaData>(RetrieveMetaData);

            private static Task<NFTMetaData> RetrieveMetaData(string id, object passedInfo)
            {
                
            }
        }
        */
    }
}

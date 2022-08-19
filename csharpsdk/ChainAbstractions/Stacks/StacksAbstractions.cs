using ChainAbstractions.Stacks.ContractWrappers;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.Metadata;
using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChainAbstractions.Stacks
{
    static public class StacksAbstractions
    {
        static public string BalanceFormatted(this IFungibleToken token)
        {
            return token.Data.FormatCount(token.Balance);
        }

        static public IFungibleToken From(this IFungibleTokenData data, double value)
        {
            var ftData = (FungibleTokenData)data;
            return new FungibleToken((ulong)(value * Math.Pow(10, ftData.Decimals)), ftData);
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

        static public IBlockchain FromAddress(string address)
        {
            if (address.StartsWith("ST"))
                return TestNet;
            return MainNet;
        }

        static public Blockchain? AsStacksBlockchain(this IBlockchain chain) => (chain as BlockchainImplBasic)?._blockchain;
        static public StacksWallet? AsStacksWallet(this IBasicWallet abstractWallet) => (abstractWallet as BasicWalletImpl)?._wallet;
        static public TransactionsManager? GetTransactionManager(this IBasicWallet abstractWallet) => (abstractWallet as BasicWalletImpl)?._manager;

        static private FTMetaDataCache? FTCache(this Blockchain chain) => (_chain2Wrapper[chain] as BlockchainImplBasic).FTCache;

        static private readonly Dictionary<Blockchain, IBlockchain> _chain2Wrapper;

        static public readonly IBlockchain MainNet;
        static public readonly IBlockchain TestNet;

        static public IFungibleTokenData Stx = new StxTokenData();

        static StacksAbstractions()
        {
            MainNet = new MainNetImpl();
            TestNet = new TestNetImpl();

            _chain2Wrapper = new Dictionary<Blockchain, IBlockchain>
            {
                {Blockchains.Mainnet, MainNet },
                {Blockchains.Testnet, TestNet },
            };
        }

        private class FungibleTokenData : IFungibleTokenData
        {
            public string Code { get; }
            public string Description { get; }
            public string ImageUrl { get; }
            public uint Decimals { get; }


            public string Address { get; }
            public string Contract { get; }
            public string Id { get; }

            public FungibleTokenData(string fullId, FungibleTokenMetaData metaData)
            {
                var tokenIdData = StacksForce.Stacks.Address.ParseFromFullTokenId(fullId);
                Address = tokenIdData.address;
                Contract = tokenIdData.contract;
                Id = tokenIdData.token;

                Code = metaData.Currency;
                if (string.IsNullOrEmpty(Code))
                    Code = tokenIdData.token;

                Description = metaData.Description;
                if (string.IsNullOrEmpty(Description))
                    Description = metaData.Name;

                ImageUrl = metaData.Image;
                Decimals = metaData.Decimals;
            }

            public FungibleTokenData(string code, string description, string imageUrl, uint decimals)
            {
                Code = code;
                Description = description;
                ImageUrl = imageUrl;
                Decimals = decimals;
            }

            public string FormatCount(ulong count) => FormatBalance(count, Decimals, Code);
        }

        private class StxTokenData : FungibleTokenData
        {
            public const string STX_CODE = "STX";
            public const uint DECIMALS = 6;
            public const string DESCRIPTION = "Stacks blockchain currency";
            public const string IMG_URL = "https://assets-global.website-files.com/618b0aafa4afde65f2fe38fe/618b0aafa4afde785dfe397d_icon-stacks-gradient.svg";

            public StxTokenData() : base(STX_CODE, DESCRIPTION, IMG_URL, DECIMALS) { }
        }

        private class FungibleToken : IFungibleToken
        {
            public ulong Balance { get; }
            public IFungibleTokenData Data => _data;

            private readonly FungibleTokenData _data;

            public FungibleToken(ulong balance, FungibleTokenData data)
            {
                Balance = balance;
                _data = data;
            }

            public string FormatCount(ulong count) => FormatBalance(count, _data.Decimals, _data.Code);
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
            internal readonly Blockchain _blockchain;

            public FTMetaDataCache FTCache { get; private set; }

            public BlockchainImplBasic(Blockchain blockchain)
            {
                _blockchain = blockchain;
                FTCache = new FTMetaDataCache(blockchain);
            }

            public IBasicWallet CreateNewWallet()
            {
                return BasicWalletImpl.CreateNew(_blockchain);
            }

            public IBasicWallet GetWalletForMnemonic(string mnemonic)
            {
                return BasicWalletImpl.FromMnemonic(_blockchain, mnemonic);
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
            internal readonly StacksWallet _wallet;
            internal readonly TransactionsManager _manager;

            static public BasicWalletImpl FromMnemonic(Blockchain chain, string mnemonic)
            {
                return new BasicWalletImpl(chain, new StacksWallet(mnemonic));
            }

            static public IBasicWallet CreateNew(Blockchain chain)
            {
                return FromMnemonic(chain, StacksWallet.GenerateMnemonicPhrase());
            }

            public BasicWalletImpl(Blockchain chain, StacksWallet wallet) : base(chain, wallet.GetAccount(0).GetAddress(chain.GetAddressVersion()))
            {
                _wallet = wallet;
                _manager = new TransactionsManager(chain, wallet.GetAccount(0));
            }

            public string GetMnemonic() => _wallet.Mnemonic;

            public async Task<ITransaction> Transfer(IFungibleToken token, string recepient, string? memo = null)
            {
                if (token.Data.Code == Stx.Code)
                {
                    var info = await _manager.StxTransfer(recepient, token.Balance, memo).ConfigureAwait(false);
                    return new TransactionInfoWrapper(info.Data, info.Error);
                } else
                {
                    var ftData = token.Data as FungibleTokenData;
                    return await SIP10.Transfer(ftData.Address, ftData.Contract, ftData.Id, this, token.Balance, GetAddress(), recepient, memo).ConfigureAwait(false);
                }
            }
        }

        internal class TransactionInfoWrapper : ITransaction
        {
            public TransactionState State { get; private set; }

            public Error? Error { get; private set; }

            private TransactionInfo _info;

            public TransactionInfoWrapper(TransactionInfo info, Error? error)
            {
                _info = info;
                Error = error;
                UpdateState();
            }

            private void UpdateState ()
            {
                if (_info == null)
                {
                    State = TransactionState.Failed;
                    return;
                }

                if (_info.Status == TransactionStatus.Pending)
                    State = TransactionState.Pending;
                else if (_info.Status == TransactionStatus.Success)
                    State = _info.IsAnchored ? TransactionState.Approved : TransactionState.PreApproved;
                else
                {
                    State = TransactionState.Failed;
                }
            }

            public override string ToString()
            {
                return Error != null ? $"Error: {Error}" : $"State = {State}"; 
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

                var result = await _chain.GetBalances(_address).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    if (currency == Stx.Code)
                        return new FungibleToken(result.Data.stx.balance, (FungibleTokenData) Stx);

                    if (result.Data.fungible_tokens.TryGetValue(currency, out var info))
                    {
                        var tokenData = await _chain.FTCache().Get(currency).ConfigureAwait(false);
                        return new FungibleToken(info.balance, tokenData);
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
                var result = await _chain.GetBalances(_address).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    foreach (var ft in result.Data.fungible_tokens)
                    {
                        var tokenData = await _chain.FTCache().Get(ft.Key).ConfigureAwait(false);
                        fts.Add(new FungibleToken(ft.Value.balance, tokenData));
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
                var result = await _chain.GetNFTHoldings(_address, new string[] { _nftType }, false, (ulong)count, (ulong)index).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    foreach (var t in result.Data.results)
                    {
                        var fullId = t.asset_identifier;
                        var data = t.Extract();

                        NFTMetaData metaData = null;
                        
                        var uri = await _chain.CallReadOnly(data.address, data.contract, "get-token-uri", _address, data.id);
                        if (uri.IsSuccess)
                        {
                            var uriStr = uri.Data.UnwrapUntil<Clarity.StringType>();
                            metaData = await NFTMetaData.FromUrl(uriStr.Value).ConfigureAwait(false);
                        }

                        if (metaData != null) { 
                            var description = metaData.Description;
                            if (string.IsNullOrEmpty(description))
                                description = data.nft;

                            nfts.Add(new NFT(metaData.Name, description, metaData.Image));
                        }
                        else
                        {
                            nfts.Add(new NFT(data.nft, "", string.Empty));
                        }
                    }
                }
                return nfts;
            }
        }

        private class FTMetaDataCache
        {
            private readonly Blockchain _chain;

            private readonly CachedDictionaryAsync<string, FungibleTokenData> _tokenId2Data;

            public FTMetaDataCache(Blockchain chain)
            {
                _chain = chain;
                _tokenId2Data = new CachedDictionaryAsync<string, FungibleTokenData>(RetrieveMetaData);
            }

            public ValueTask<FungibleTokenData> Get(string tokenId) => _tokenId2Data.Get(tokenId);

            private async Task<FungibleTokenData> RetrieveMetaData(string id, object passedInfo)
            {
                var parsed = Address.ParseFromFullTokenId(id);
                var metaData = await FungibleTokenMetaData.ForTokenContract(_chain, parsed.address + "." + parsed.contract);
                return new FungibleTokenData(id, metaData);
            }
        }
    }
}

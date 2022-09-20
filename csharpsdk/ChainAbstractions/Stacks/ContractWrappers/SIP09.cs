using StacksForce.Stacks.WebApi;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;
using System.Numerics;
using System.Threading.Tasks;

namespace ChainAbstractions.Stacks.ContractWrappers
{
    // https://github.com/stacksgov/sips/blob/main/sips/sip-009/sip-009-nft-standard.md
    public class SIP09<TokenIdType> where TokenIdType : Clarity.Value
    {
        private readonly string _address;
        private readonly string _contract;
        private readonly string _token;

        public SIP09(string address, string contract, string token)
        {
            _address = address;
            _contract = contract;
            _token = token;
        }

        public SIP09(string fullId)
        {
            var data = Address.ParseFromFullTokenId(fullId);
            _address = data.address;
            _contract = data.contract;
            _token = data.token;
        }

        static public async Task<ITransaction> Transfer(string address, string contract, string nft, IBasicWallet caller, string recepient, TokenIdType tokenId)
        {
            var manager = caller.GetTransactionManager();
            
            var transaction = await manager.GetContractCall(address, contract, "transfer",
                new Clarity.Value[] {
                    tokenId,
                    Clarity.Principal.FromString(caller.GetAddress()),
                    Clarity.Principal.FromString(recepient),
                },
                new PostCondition[]
                {
                    new NFTPostCondition(recepient, new AssetInfo(address, contract, nft), NonFungibleConditionCode.Owns, tokenId)
                }).ConfigureAwait(false);

            return new StacksAbstractions.TransactionWrapper(manager, transaction);
        }

        static public Task<AsyncCallResult<string?>> GetTokenUri(string address, string contract, TokenIdType tokenId)
        {
            var chain = StacksAbstractions.FromAddress(address);
            return WebApiHelpers.ReadonlyGetString(chain.AsStacksBlockchain(), address, contract, "get-token-uri", tokenId);
        }

        static public async Task<AsyncCallResult<string?>> GetTokenOwner(string address, string contract, TokenIdType tokenId)
        {
            var chain = StacksAbstractions.FromAddress(address);
            var owner = await WebApiHelpers.ReadonlyGet<Clarity.Principal>(chain.AsStacksBlockchain(), address, contract, "get-owner", tokenId).ConfigureAwait(false);
            if (owner.IsSuccess)
                return owner.Data != null ? new AsyncCallResult<string?>(owner.Data!.ToString()) : new AsyncCallResult<string?>((string?)null);
            return new AsyncCallResult<string?>(owner.Error!);
        }

        public Task<ITransaction> Transfer(IBasicWallet caller, string recepient, TokenIdType tokenId) =>
            Transfer(_address, _contract, _token, caller, recepient, tokenId);

        public Task<AsyncCallResult<string?>> GetTokenUri(TokenIdType tokenId) => GetTokenUri(_address, _contract, tokenId);
        public Task<AsyncCallResult<string?>> GetTokenOwner(TokenIdType tokenId) => GetTokenOwner(_address, _contract, tokenId);

        public Task<INFT> GetById(TokenIdType tokenId) => NFTUtils.GetFrom(_address, _contract, _token, tokenId);
    }

    public class SIP09UnsignedInteger
    {
        private readonly SIP09<Clarity.UInteger128> _inner;

        public SIP09UnsignedInteger(string address, string contract, string token)
        {
            _inner = new SIP09<Clarity.UInteger128>(address, contract, token);
        }

        public SIP09UnsignedInteger(string fullId)
        {
            _inner = new SIP09<Clarity.UInteger128>(fullId);
        }

        public Task<ITransaction> Transfer(IBasicWallet caller, string recepient, BigInteger tokenId) =>
            _inner.Transfer(caller, recepient, new Clarity.UInteger128(tokenId));

        public Task<AsyncCallResult<string?>> GetTokenUri(BigInteger tokenId) => _inner.GetTokenUri(new Clarity.UInteger128(tokenId));
        public Task<AsyncCallResult<string?>> GetTokenOwner(BigInteger tokenId) => _inner.GetTokenOwner(new Clarity.UInteger128(tokenId));

        public Task<ITransaction> Transfer(IBasicWallet caller, string recepient, ulong tokenId) =>
           _inner.Transfer(caller, recepient, new Clarity.UInteger128(tokenId));

        public Task<AsyncCallResult<string?>> GetTokenUri(ulong tokenId) => _inner.GetTokenUri(new Clarity.UInteger128(tokenId));
        public Task<AsyncCallResult<string?>> GetTokenOwner(ulong tokenId) => _inner.GetTokenOwner(new Clarity.UInteger128(tokenId));

        public Task<INFT> GetById(ulong tokenId) => _inner.GetById(new Clarity.UInteger128(tokenId));
    }
}

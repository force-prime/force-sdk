using System.Text;
using System.Threading.Tasks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;
using StacksForce.Utils;

namespace ChainAbstractions.Stacks.ContractWrappers
{
    public class SIP10
    {
        private readonly string _address;
        private readonly string _contract;
        private readonly string _token;

        public SIP10(string address, string contract, string token)
        {
            _address = address;
            _contract = contract;
            _token = token; 
        }

        static public async Task<ITransaction> Transfer(string address, string contract, string token, IBasicWallet caller, ulong amount, string sender, string recepient, string? memo = null)
        {
            byte[]? bytes = null;
            if (!string.IsNullOrEmpty(memo))
            {
                bytes = Encoding.ASCII.GetBytes(memo);
                if (bytes.Length > 34)
                    throw new System.ArgumentException("memo is too long (34 bytes max)");
            }

            Clarity.Value memoValue = bytes == null ? (Clarity.Value)new Clarity.None() : new Clarity.ByteBuffer(bytes);

            var result = await caller.GetTransactionManager().ContractCall(address, contract, "transfer",
                new Clarity.Value[] {
                    new Clarity.UInteger128(amount),
                    Clarity.Principal.FromString(sender),
                    Clarity.Principal.FromString(recepient),
                    new Clarity.OptionalSome(memoValue)
                },
                new PostCondition[]
                {
                    new FungibleTokenPostCondition(sender, new AssetInfo(address, contract, token), FungibleConditionCode.Equal, amount),
                }).ConfigureAwait(false);

            return new StacksAbstractions.TransactionInfoWrapper(result.Data, result.Error);
        }

        public Task<ITransaction> Transfer(IBasicWallet caller, ulong amount, string sender, string recepient, string? memo = null) =>
            Transfer(_address, _contract, _token, caller, amount, sender, recepient, memo);
     
        public async Task<AsyncCallResult<string>> GetName()
        {
            var chain = StacksAbstractions.FromAddress(_address);

            var result = await chain.AsStacksBlockchain().CallReadOnly(_address, _contract, "get-name", _address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<string>(result.Error);

            return new AsyncCallResult<string>(result.Data.UnwrapUntil<Clarity.StringType>().Value);
        }

        public async Task<AsyncCallResult<string>> GetSymbol()
        {
            var chain = StacksAbstractions.FromAddress(_address);

            var result = await chain.AsStacksBlockchain().CallReadOnly(_address, _contract, "get-symbol", _address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<string>(result.Error);

            return new AsyncCallResult<string>(result.Data.UnwrapUntil<Clarity.StringType>().Value);
        }

        public async Task<AsyncCallResult<uint>> GetDecimals()
        {
            var chain = StacksAbstractions.FromAddress(_address);

            var result = await chain.AsStacksBlockchain().CallReadOnly(_address, _contract, "get-decimals", _address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<uint>(result.Error);

            return new AsyncCallResult<uint>((uint) result.Data.UnwrapUntil<Clarity.UInteger128>().Value);
        }

        public async Task<AsyncCallResult<string>> GetTokenUri()
        {
            var chain = StacksAbstractions.FromAddress(_address);

            var result = await chain.AsStacksBlockchain().CallReadOnly(_address, _contract, "get-token-uri", _address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<string>(result.Error);

            return new AsyncCallResult<string>(result.Data.UnwrapUntil<Clarity.StringType>().Value);
        }
    }
}

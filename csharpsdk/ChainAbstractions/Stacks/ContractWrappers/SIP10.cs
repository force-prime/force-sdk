using System.Text;
using System.Threading.Tasks;
using StacksForce.Stacks.WebApi;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;

namespace ChainAbstractions.Stacks.ContractWrappers
{
    // https://github.com/stacksgov/sips/blob/main/sips/sip-010/sip-010-fungible-token-standard.md
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

            var manager = caller.GetTransactionManager();

            var transaction = await manager.GetContractCall(address, contract, "transfer",
                new Clarity.Value[] {
                    new Clarity.UInteger128(amount),
                    Clarity.Principal.FromString(sender),
                    Clarity.Principal.FromString(recepient),
                    new Clarity.OptionalSome(memoValue)
                },
                new PostCondition[]
                {
                    new FungibleTokenPostCondition(sender, new AssetInfo(address, contract, token), FungibleConditionCode.Equal, amount),
                }).ConfigureAwait();

            return new StacksAbstractions.TransactionWrapper(manager, transaction);
        }

        public Task<ITransaction> Transfer(IBasicWallet caller, ulong amount, string sender, string recepient, string? memo = null) =>
            Transfer(_address, _contract, _token, caller, amount, sender, recepient, memo);

        public Task<AsyncCallResult<string?>> GetName() => GetString("get-name");

        public Task<AsyncCallResult<string?>> GetSymbol() => GetString("get-symbol");

        public async Task<AsyncCallResult<uint>> GetDecimals()
        {
            var chain = StacksAbstractions.FromAddress(_address);

            var result = await WebApiHelpers.ReadonlyGetUlong(chain.AsStacksBlockchain(), _address, _contract, "get-decimals").ConfigureAwait();

            if (result.IsError)
                return result.Error!;

            return (uint) result.Data!.Value;
        }

        public Task<AsyncCallResult<string?>> GetTokenUri() => GetString("get-token-uri");

        private Task<AsyncCallResult<string?>> GetString(string method)
        {
            var chain = StacksAbstractions.FromAddress(_address);
            return WebApiHelpers.ReadonlyGetString(chain.AsStacksBlockchain(), _address, _contract, method);
        }
    }
}

using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class WebApiHelpers
    {
        static public async Task<AsyncCallResult<string?>> ReadonlyGetString(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments)
        {
            var result = await ReadonlyGet<Clarity.StringType>(chain, address, contract, method, arguments).ConfigureAwait();

            if (result.IsSuccess)
                return result.Data != null ? new AsyncCallResult<string?>(result.Data.Value) : new AsyncCallResult<string?>((string?) null);

            return new AsyncCallResult<string?>(result.Error!);
        }

        static public async Task<AsyncCallResult<BigInteger?>> ReadonlyGetUlong(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments)
        {
            var result = await ReadonlyGet<Clarity.UInteger128>(chain, address, contract, method, arguments).ConfigureAwait();

            if (result.IsSuccess)
                return new AsyncCallResult<BigInteger?>(result.Data!.Value);

            return new AsyncCallResult<BigInteger?>(result.Error!);
        }

        static public async Task<AsyncCallResult<T?>> ReadonlyGet<T>(this Blockchain chain, string address, string contract, string method, params Clarity.Value[] arguments) where T: Clarity.Value
        {
            var result = await chain.CallReadOnly(address, contract, method, address, arguments).ConfigureAwait();
            if (result.IsSuccess)
                return new AsyncCallResult<T?>(result.Data!.UnwrapUntil<T>());

            return new AsyncCallResult<T?>(result.Error!);
        }
    }


    public class TransactionEventStream : BasicDataStream<TransactionEvent>
    {
        private readonly Blockchain _chain;
        private readonly string _txId;

        public TransactionEventStream(Blockchain chain, string txId)
        {
            _chain = chain;
            _txId = txId;
        }

        protected async override Task<List<TransactionEvent>?> GetRange(long index, long count)
        {
            var result = await _chain.GetTransactionEvents(_txId, (int) count, (int) index).ConfigureAwait();
            
            if (result.IsSuccess)
            {
                return result.Data.events.Select(x => TransactionEvent.FromEventData(x)).Where(x => x != null).ToList();
            }
            return null;
        }
    }

    public class BlockTransactionsStream : BasicDataStream<TransactionInfo>
    {
        private readonly Blockchain _chain;
        private readonly uint _blockHeight;

        public BlockTransactionsStream(Blockchain chain, uint blockHeight)
        {
            _chain = chain;
            _blockHeight = blockHeight;
        }

        protected async override Task<List<TransactionInfo>?> GetRange(long index, long count)
        {
            var result = await _chain.GetBlockTransactions(_blockHeight, (int) count, (int)index).ConfigureAwait();
            if (result.IsError)
                return null;

            return result.Data.results.Select(x => TransactionInfo.FromData(_chain, x)).ToList();
        }
    }

    public class TransactionInfoStream : BasicDataStream<TransactionInfo>
    {
        private readonly Blockchain _chain;
        private readonly bool _unanchored;

        public TransactionInfoStream(Blockchain chain, bool unanchored = true)
        {
            _chain = chain;
            _unanchored = unanchored;
        }

        protected async override Task<List<TransactionInfo>?> GetRange(long index, long count)
        {
            var result = await _chain.GetRecentTransactions((uint) count, (uint) index, null, _unanchored).ConfigureAwait();
            List<TransactionInfo> transactions = new List<TransactionInfo>();
            if (result.IsSuccess)
            {
                var ids = result.Data.results.Select(x => x.tx_id).ToArray();
                var transactionsWithEvents = await _chain.GetTransactionsDetails(ids, 0, 30, _unanchored).ConfigureAwait();
                if (transactionsWithEvents.IsSuccess)
                {
                    foreach (var t in transactionsWithEvents.Data)
                    {
                        if (t.Value.found)
                        {
                            var info = TransactionInfo.FromData(_chain, t.Value.result);
                            if (info != null)
                                transactions.Add(info);
                        }
                    }
                }
                else
                    return null;
            }
            else 
                return null;
            return transactions;
        }
    }
}

using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StacksForce.Stacks.ChainTransactions
{
    public class TransactionsManager
    {
        public Blockchain Chain { get; }
        public StacksAccount Sender { get; }

        private readonly Dictionary<string, Transaction> _id2Transaction = new Dictionary<string, Transaction>();
        private readonly Dictionary<string, TransactionInfo> _id2Info = new Dictionary<string, TransactionInfo>();

        public TransactionsManager(Blockchain chain, StacksAccount sender)
        {
            Chain = chain;
            Sender = sender;
        }

        public Task<AsyncCallResult<Transaction>> GetStxTransfer(string recepient, ulong amount, string? memo = null)
        {
            var transaction = TransactionBuilder.StxTransfer(Chain, Sender, recepient, amount, memo);
            return TransactionUtils.Prepare(Chain, Sender, transaction);
        }

        public Task<AsyncCallResult<Transaction>> GetContractCall(string address, string contract, string function, Clarity.Value[]? arguments = null, PostCondition[]? postConditions = null)
        {
            var transaction = TransactionBuilder.ContractCall(Chain, Sender, address, contract, function, arguments, postConditions);
            return TransactionUtils.Prepare(Chain, Sender, transaction);
        }

        public Task<AsyncCallResult<Transaction>> GetContractDeploy(string contractName, string code)
        {
            var transaction = TransactionBuilder.DeployContract(Chain, Sender, contractName, code);
            return TransactionUtils.Prepare(Chain, Sender, transaction);
        }

        public List<T> GetByStatus<T>(TransactionStatus status) where T : TransactionInfo {
            lock (_id2Info)
            {
                return _id2Info.Where(kv => kv.Value is T && kv.Value.Status == status).Select(kv => kv.Value).Cast<T>().ToList();
            }
        }

        public async Task<Error?> FetchFromMempool()
        {
            var result = await Chain.GetMempoolTransactions(Sender.GetAddress(Chain.GetAddressVersion())).ConfigureAwait();
            if (result.IsError)
                return result.Error!;

            foreach (var t in result.Data.results)
            {
                lock (_id2Info)
                {
                    var pending = TransactionInfo.FromData(Chain, t);
                    if (pending != null)
                        _id2Info.Add(t.tx_id, pending);
                }
            }

            return null;
        }

        public async Task<AsyncCallResult<TransactionInfo>> ResendTransaction(string txid, ulong newFee)
        {
            TransactionInfo? currentInfo = null;
            Transaction? t = null;
            lock (_id2Info)
            {
                _id2Info.TryGetValue(txid, out currentInfo);
                _id2Transaction.TryGetValue(txid, out t);
                if (currentInfo == null)
                    return new AsyncCallResult<TransactionInfo>(new Error("Not found"));
            }

            if (currentInfo.Status != TransactionStatus.Pending)
                return new AsyncCallResult<TransactionInfo>(new Error("Incorrect status"));

            t.UpdateFeeAndNonce(newFee, t.Nonce);

            return await Run(t).ConfigureAwait();
        }

        public async Task<AsyncCallResult<TransactionInfo>> Run(Transaction transaction)
        {
            var result = await Chain.RunTransaction(Sender, transaction).ConfigureAwait();
            if (result.IsError)
                return new AsyncCallResult<TransactionInfo>(result.Error!);

            Log.Debug($"TransactionManager broadcasted: {transaction.Nonce} {transaction.Fee} {transaction.TransactionType}");

            var txId = result.Data;

            lock (_id2Info) { 
                if (_id2Info.TryGetValue(txId, out var t)) // send the same transaction
                {
                    return new AsyncCallResult<TransactionInfo>(new AlreadyBroadcastedError(txId));
                }
            }
            
            var pending = TransactionInfo.GetPending(Chain, transaction.TransactionType, txId)!;
            Chain.GetTransactionMonitor().WatchTransaction(pending);

            lock (_id2Info)
            {
                _id2Transaction.Add(txId, transaction);
                _id2Info.Add(txId, pending);
            }

            return new AsyncCallResult<TransactionInfo>(pending);
        }

        public class AlreadyBroadcastedError : Error
        {
            public string TxId { get; }
            public AlreadyBroadcastedError(string txId) : base("AlreadyBroadcastedError")
            {
                TxId = txId;
            }
        }
    }
}

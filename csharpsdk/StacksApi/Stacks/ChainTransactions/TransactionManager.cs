using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static StacksForce.Stacks.WebApi.HttpAPIUtils;

namespace StacksForce.Stacks.ChainTransactions
{
    public class TransactionsManager
    {
        static public bool MonitorTransactionState = true;
        static public ulong DefaultTransactionCostUstx = 10000;

        public Blockchain Chain { get; }
        public StacksAccountBase Sender { get; }

        private readonly Dictionary<string, Transaction> _id2Transaction = new Dictionary<string, Transaction>();
        private readonly Dictionary<string, TransactionInfo> _id2Info = new Dictionary<string, TransactionInfo>();
        
        private readonly SemaphoreSlim _isFree = new SemaphoreSlim(1);
        private long _lastSuccessNonce = -1;

        public TransactionsManager(Blockchain chain, StacksAccountBase sender)
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
                    return new Error("NotFound");
            }

            if (currentInfo.Status != TransactionStatus.Pending)
                return new Error("IncorrectStatus");

            t.UpdateFeeAndNonce(newFee, t.Nonce);

            return await Run(t).ConfigureAwait();
        }

        public async Task<AsyncCallResult<TransactionInfo>> Run(Transaction transaction)
        {
            await _isFree.WaitAsync().ConfigureAwait(); // process transaction requests continuously

            var r = await Chain.Prepare(Sender, transaction).ConfigureAwait();
            if (r.IsError)
            {
                _isFree.Release();
                return r.Error!;
            }

            var baseNonce = transaction.Nonce;

            AsyncCallResult<string> result;

            while (true)
            {
                if ((long) transaction.Nonce <= _lastSuccessNonce)
                    transaction.UpdateFeeAndNonce(transaction.Fee, (ulong) _lastSuccessNonce);

                result = await Chain.SignAndBroadcast(Sender, transaction).ConfigureAwait();

                if (result.IsSuccess)
                {
                    _lastSuccessNonce = (long)transaction.Nonce;
                    break;
                }
                else if (result.Error is ConflictingNonceInMempoolError)
                {
                    if (transaction.Nonce < baseNonce + 3)
                        transaction.UpdateFeeAndNonce(transaction.Fee, transaction.Nonce + 1);
                    else break;
                }
                else break;
            }

            _isFree.Release();

            if (result.IsError)
                return result.Error!;

            Log.Debug($"TransactionManager broadcasted: {transaction.Nonce} {transaction.Fee} {transaction.TransactionType}");

            var txId = result.Data;

            lock (_id2Info) { 
                if (_id2Info.TryGetValue(txId, out var t)) // send the same transaction
                {
                    return new AlreadyBroadcastedError(txId);
                }
            }
            
            var pending = TransactionInfo.GetPending(Chain, transaction.TransactionType, txId)!;

            if (MonitorTransactionState)
                Chain.GetTransactionMonitor().WatchTransaction(pending);

            lock (_id2Info)
            {
                _id2Transaction.Add(txId, transaction);
                _id2Info.Add(txId, pending);
            }

            return pending;
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

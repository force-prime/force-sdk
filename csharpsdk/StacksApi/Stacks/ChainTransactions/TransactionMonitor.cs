using StacksForce.Stacks.WebApi;
using System.Collections.Concurrent;

namespace StacksForce.Stacks.ChainTransactions
{
    public class TransactionMonitor
    {
        private readonly WebSocketAPI _ws;

        private readonly ConcurrentDictionary<string, TransactionInfo> _id2Info = new ConcurrentDictionary<string, TransactionInfo>();

        internal TransactionMonitor(Blockchain chain)
        {
            _ws = new WebSocketAPI(chain);

            _ws.SubsribeToBlockEvent();

            _ws.OnNewBlock += OnNewBlock;
            _ws.OnTxUpdated += OnTxUpdated;
        }

        public void WatchTransaction(TransactionInfo transactionInfo)
        {
            _ws.Connect(false);

            if (_id2Info.TryAdd(transactionInfo.TxId, transactionInfo))
                _ws.SubsribeToTransactionUpdate(transactionInfo.TxId);
        }

        private async void OnNewBlock()
        {
            foreach (var info in _id2Info.Values)
            {
                await info.Refresh().ConfigureAwait(false);
                RemoveCompletedAndAnchored(info);
            }
        }

        private async void OnTxUpdated(string txId, TransactionStatus status)
        {
            if (_id2Info.TryGetValue(txId, out var info))
            {
                // TODO: status is not reported correctly (always pending), at least for test net nodes
                // info.UpdateStatus(status);
                await info.Refresh().ConfigureAwait(false);
                RemoveCompletedAndAnchored(info);
            }
        }

        private void RemoveCompletedAndAnchored(TransactionInfo transactionInfo)
        {
            if (transactionInfo.Status == TransactionStatus.Undefined || transactionInfo.Status == TransactionStatus.Pending)
                return;

            if (transactionInfo.Status != TransactionStatus.Success || transactionInfo.IsAnchored)
                _id2Info.TryRemove(transactionInfo.TxId, out var _);
        }
    }

    static public class TransactionMonitorExt {
        static private TransactionMonitor TestNet = new TransactionMonitor(Blockchains.Testnet);
        static private TransactionMonitor MainNet = new TransactionMonitor(Blockchains.Mainnet);
        static public TransactionMonitor GetTransactionMonitor(this Blockchain chain)
        {
            if (chain == Blockchains.Mainnet)
                return MainNet;
            return TestNet;
        }
    }
}

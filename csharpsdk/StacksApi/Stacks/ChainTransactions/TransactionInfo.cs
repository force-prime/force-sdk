using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StacksForce.Stacks.ChainTransactions.TransactionEvent;
using static StacksForce.Stacks.WebApi.Transactions;
using static StacksForce.Stacks.WebApi.Transactions.TransactionData;

namespace StacksForce.Stacks.ChainTransactions
{
    public class TransactionInfo
    {
        public TransactionStatus Status { get; private set; } = TransactionStatus.Undefined;
        public TransactionType Type { get; private set; } = TransactionType.Undefined;
        public uint MicroblockSequence { get; private set; }
        public ulong Nonce { get; private set; }
        public ulong Fee { get; private set; }
        public bool IsAnchored { get; private set; }
        public string TxId { get; private set; }
        public string MicroblockHash { get; private set; }
        public string Sender { get; private set; }
        public DateTime BurnBlockTime { get; private set; }
        public uint BlockHeight { get; private set; }

        public IDataStreamProvider<TransactionEvent> Events => _events;

        public event Action? StatusChanged;

        private Blockchain _chain;
        private IDataStreamProvider<TransactionEvent> _events;

        public string StacksExplorerLink => $"https://explorer.stacks.co/txid/{TxId}?chain={_chain.Name.ToLower()}";

        static internal TransactionInfo? FromData(Blockchain chain, TransactionData transactionData)
        {
            var type = EnumUtils.FromString(transactionData.tx_type, TransactionType.Undefined);
            var info = GetPending(chain, type, transactionData.tx_id);

            if (info != null)
            {
                info.RefreshFromData(transactionData);
            }
            return info;
        }

        static internal TransactionInfo? GetPending(Blockchain chain, TransactionType type, string txId)
        {
            var info = FromType(type);
            if (info != null)
            {
                info.Status = TransactionStatus.Pending;
                info.Type = type;
                info.TxId = txId;
                info._chain = chain;
            }
            return info;
        }

        static public async Task<TransactionInfo?> ForTxId(Blockchain chain, string txId) {
            var result = await GetTxInfo(chain, txId).ConfigureAwait();
            if (result.IsError)
                return null;

            var type = EnumUtils.FromString(result.Data.tx_type, TransactionType.Undefined);

            var info = GetPending(chain, type, txId);
            if (info != null)
                info.RefreshFromData(result.Data);

            return info;
        }

        public async Task<Error?> Refresh()
        {
            var result = await GetTxInfo(_chain, TxId).ConfigureAwait();
            if (result.IsError)
                return result.Error;

            RefreshFromData(result.Data);

            return null;
        }

        static private TransactionInfo? FromType(TransactionType type) =>
            type switch
            {
                TransactionType.TokenTransfer => new TransferTransactionInfo(),
                TransactionType.ContractCall => new ContractCallTransactionInfo(),
                TransactionType.SmartContract => new ContractDeployTransactionInfo(),
                TransactionType.Coinbase => new CoinbaseTransactionInfo(),
                _ => null
            };


        private static async Task<AsyncCallResult<TransactionData>> GetTxInfo(Blockchain chain, string txId)
        {
            if (!txId.StartsWith("0x"))
                txId = "0x" + txId;

            var result = await chain.GetTransactionsDetails(new string[] { txId }, 0, 50, true).ConfigureAwait();
            if (result.IsError)
                return new AsyncCallResult<TransactionData>(result.Error!);

            var response = result.Data[txId];
            if (!response.found)
                return new AsyncCallResult<TransactionData>(new Error("Not found"));

            return new AsyncCallResult<TransactionData>(response.result);
        }

        protected virtual void RefreshFromData(TransactionData transactionData)
        {
            UpdateEvents(transactionData);

            if (transactionData.burn_block_time > 0)
                BurnBlockTime = DateTimeOffset.FromUnixTimeSeconds(transactionData.burn_block_time).DateTime;
            BlockHeight = transactionData.block_height;
            MicroblockSequence = transactionData.microblock_sequence;
            MicroblockHash = transactionData.microblock_hash;
            IsAnchored = !transactionData.is_unanchored && transactionData.tx_status == "success";
            Nonce = transactionData.nonce;
            Fee = transactionData.fee_rate;
            Sender = transactionData.sender_address;

            var status = EnumUtils.FromString(transactionData.tx_status, TransactionStatus.Undefined);
            UpdateStatus(status);
        }

        private void UpdateEvents(TransactionData transactionData)
        {
            if (_events != null)
                return;

            bool isEmpty = transactionData.event_count == 0;
            if (isEmpty)
            {
                _events = EmptyDataStreamProvider<TransactionEvent>.EMPTY;
                return;
            }
                 
            bool allEvents = isEmpty;
            List<TransactionEvent>? events = null;

            if (transactionData.events != null && !allEvents)
            {
                allEvents = transactionData.events.Length == transactionData.event_count;
                events = new List<TransactionEvent>();
                foreach (var e in transactionData.events)
                {
                    TransactionEvent? evt = FromEventData(e);
                    if (evt != null)
                        events.Add(evt);
                }
            }

            if (allEvents)
                _events = new BasicCachedDataStream<TransactionEvent>(events!);
            else
                _events = new BasicCachedDataStream<TransactionEvent>(new TransactionEventStream(_chain, TxId), events);
        }

        internal void UpdateStatus(TransactionStatus newStatus)
        {
            if (Status == newStatus)
                return;

            Status = newStatus;
            StatusChanged?.Invoke();
        }
    }

    public class ContractDeployTransactionInfo: TransactionInfo
    {
        public string Contract { get; private set; }

        protected override void RefreshFromData(TransactionData result)
        {
            var addressAndContract = result.smart_contract.contract_id.Split('.');
            Contract = addressAndContract[1];

            base.RefreshFromData(result);
        }

        public override string ToString()
        {
            return $"Deploy: {Sender}.{Contract}";
        }
    }

    public class ContractCallTransactionInfo: TransactionInfo
    {
        public string Address { get; private set; }
        public string Contract { get; private set; }
        public string Function { get; private set; }

        public Clarity.Value? Result { get; private set; }

        public Clarity.Value[]? Arguments { get; private set; }

        public override string ToString()
        {
            string arguments = Arguments != null ? string.Join(", ", Arguments.Select(x => x.ToString())) : String.Empty;
            return $"Call {Address}.{Contract}::{Function}({arguments})";
        }

        protected override void RefreshFromData(TransactionData result)
        {
            var addressAndContract = result.contract_call.contract_id.Split('.');
            var arguments = result.contract_call.function_args.Select(x => Clarity.Value.FromHex(x.hex)).ToArray();
            Clarity.Value? callResult = result.tx_result != null ? Clarity.Value.FromHex(result.tx_result.hex) : null;

            Address = addressAndContract[0];
            Contract = addressAndContract[1];
            Function = result.contract_call.function_name;
            Arguments = arguments;
            Result = callResult;

            base.RefreshFromData(result);
        }
    }

    public class CoinbaseTransactionInfo : TransactionInfo
    {

    }

    public class TransferTransactionInfo : TransactionInfo
    {
        public string Recepient { get; private set; }
        public string Memo { get; private set; }
        public BigInteger Amount { get; private set; }

        protected override void RefreshFromData(TransactionData result)
        {
            Recepient = result.token_transfer.recipient_address;
            Memo = ExtractMemo(result.token_transfer.memo);
            Amount = result.token_transfer.amount;

            base.RefreshFromData(result);
        }

        static private string ExtractMemo(string hexedMemo)
        {
            var bytes = hexedMemo.ToHexByteArray().TrimEnd();
            var memo = bytes.Length > 0 ? Encoding.ASCII.GetString(bytes) : String.Empty;
            return memo;
        }

        public override string ToString()
        {
            return $"Transfer {Amount} {Sender} -> {Recepient} ({Memo})";
        }
    }

    public class TransactionEvent
    {
        public enum EventType
        {
            Undefined,

            StxAsset,
            FungibleTokenAsset,
            NonFungibleTokenAsset,
            StxLock,
            SmartContractLog
        }

        public enum TokenEventType
        {
            Undefined,

            Transfer,
            Mint,
            Burn
        }
        static public TransactionEvent? FromEventData(TransactionData.Event e)
        {
            var type = EnumUtils.FromString(e.event_type, EventType.Undefined);
            switch (type)
            {
                case EventType.StxAsset:
                    return StxEvent.From(e.asset);
                case EventType.NonFungibleTokenAsset:
                    return NFTEvent.From(e.asset);
                case EventType.SmartContractLog:
                    return LogEvent.From(e.contract_log);
                case EventType.FungibleTokenAsset:
                    return FTEvent.From(e.asset);
                default:
                    Log.Trace("Transaction event type not supported " + e.event_type);
                    return null;
            }
        }
    }

    public class FTEvent : TokenEvent
    {
        public ulong Amount { get; private set; }

        internal static FTEvent? From(TransactionEventAsset data)
        {
            var e = From<FTEvent>(data);
            if (e == null)
                return null;
            e.Amount = data.amount;
            return e;
        }
        public override string ToString()
        {
            return $"{Type} {AssetId} {Amount}";
        }
    }

    public class NFTEvent : TokenEvent
    {
        internal static NFTEvent? From(TransactionEventAsset data) => From<NFTEvent>(data);
    }

    public class TokenEvent : TransactionEvent
    {
        public TokenEventType Type { get; private set; }
        public string AssetId { get; private set; }
        internal static T? From<T>(TransactionEventAsset data) where T: TokenEvent, new()
        {
            if (data == null) return null;

            var e = new T();
            var status = EnumUtils.FromString(data.asset_event_type, TokenEventType.Undefined);
            if (status == TokenEventType.Undefined)
                return null;
            e.Type = status;
            e.AssetId = data.asset_id;
            return e;
        }

        public override string ToString()
        {
            return $"{Type} {AssetId}";
        }
    }

    public class LogEvent : TransactionEvent
    {
        public Clarity.Value Value { get; private set; }

        internal static LogEvent? From(ContractLog data)
        {
            if (data == null || data.value == null) return null;
            var value = Clarity.Value.FromHex(data.value.hex);
            if (value == null || value.IsNone())
                return null;
            return new LogEvent { Value = value };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class StxEvent : TransactionEvent
    {
        public ulong Amount { get; private set; }
        public TokenEventType Type { get; private set; }

        internal static StxEvent? From(TransactionData.TransactionEventAsset data)
        {
            if (data == null) return null;

            var e = new StxEvent();
            var status = EnumUtils.FromString(data.asset_event_type, TokenEventType.Undefined);
            if (status == TokenEventType.Undefined)
                return null;
            e.Type = status;
            e.Amount = data.amount;
            return e;
        }
        public override string ToString()
        {
            return $"{Type} {Amount} mSTXs";
        }

    }
}

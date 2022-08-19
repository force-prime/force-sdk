using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StacksForce.Stacks.WebApi.Transactions;

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

        public event Action? StatusChanged;

        private Blockchain _chain;

        static public async Task<int> GetMicroblockConfirmations(Blockchain chain, string microblockId)
        {
            if (string.IsNullOrEmpty(microblockId))
                return 0;

            var result = await chain.GetRecentMicroblocks();
            if (result.IsError)
                return -1;

            uint mySeq = 0;
            uint maxMs = 0;

            foreach (var m in result.Data.results)
            {
                if (m.microblock_hash == microblockId)
                    mySeq = m.microblock_sequence;

                maxMs = Math.Max(maxMs, m.microblock_sequence);
            }

            return (int)(maxMs - mySeq);
        }

        static internal TransactionInfo? FromData(Blockchain chain, GetTransactionDetailsReponse.Transaction transaction)
        {
            var type = EnumUtils.FromString(transaction.tx_type, TransactionType.Undefined);
            var info = GetPending(chain, type, transaction.tx_id);
            if (info != null)
                info.RefreshFromData(transaction);
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
            var result = await GetTxInfo(chain, txId).ConfigureAwait(false);
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
            var result = await GetTxInfo(_chain, TxId).ConfigureAwait(false);
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
                _ => null
            };


        private static async Task<AsyncCallResult<GetTransactionDetailsReponse.Transaction>> GetTxInfo(Blockchain chain, string txId)
        {
            if (!txId.StartsWith("0x"))
                txId = "0x" + txId;

            var result = await chain.GetTransactionsDetails(new string[] { txId }, 0, 96, true).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<GetTransactionDetailsReponse.Transaction>(result.Error!);

            var response = result.Data[txId];
            if (!response.found)
                return new AsyncCallResult<GetTransactionDetailsReponse.Transaction>(new Error("Not found"));

            return new AsyncCallResult<GetTransactionDetailsReponse.Transaction>(response.result);
        }

        protected virtual void RefreshFromData(GetTransactionDetailsReponse.Transaction result)
        {
            MicroblockSequence = result.microblock_sequence;
            MicroblockHash = result.microblock_hash;
            IsAnchored = !result.is_unanchored && result.tx_status == "success";
            Nonce = result.nonce;
            Fee = result.fee_rate;
            Sender = result.sender_address;

            var status = EnumUtils.FromString(result.tx_status, TransactionStatus.Undefined);
            UpdateStatus(status);
        }

        private void UpdateStatus(TransactionStatus newStatus)
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

        protected override void RefreshFromData(GetTransactionDetailsReponse.Transaction result)
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

        public Clarity.Value[] Arguments { get; private set; }

        public override string ToString()
        {
            string arguments = Arguments != null ? string.Join(", ", Arguments.Select(x => x.ToString())) : String.Empty;
            return $"Call {Address}.{Contract}::{Function}({arguments})";
        }

        protected override void RefreshFromData(GetTransactionDetailsReponse.Transaction result)
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

    public class TransferTransactionInfo : TransactionInfo
    {
        public string Recepient { get; private set; }
        public string Memo { get; private set; }
        public BigInteger Amount { get; private set; }

        protected override void RefreshFromData(GetTransactionDetailsReponse.Transaction result)
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
}

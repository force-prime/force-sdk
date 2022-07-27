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
        public bool IsAnchored { get; private set; }
        public string TxId { get; private set; }
        public string MicroblockHash { get; private set; }

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

        static public async Task<TransactionInfo?> ForTxId(Blockchain chain, string txId) {
            if (!txId.StartsWith("0x"))
                txId = "0x" + txId;

            var result = await chain.GetTransactionsDetails(new string[] { txId }, 0, 96, true);
            if (result.IsError)
                return null;

            var response = result.Data[txId];
            if (!response.found)
                return null;

            var type = EnumUtils.FromString(response.result.tx_type, TransactionType.Undefined);

            TransactionInfo? info = type switch
            {
                TransactionType.TokenTransfer => TransferFrom(response.result),
                TransactionType.ContractCall => ContractCallFrom(response.result),
                _ => null
            };

            if (info != null)
            {
                info.TxId = txId;

                var status = EnumUtils.FromString(response.result.tx_status, TransactionStatus.Undefined);
                info.UpdateStatus(status);

                info.MicroblockSequence = response.result.microblock_sequence;
                info.IsAnchored = !response.result.is_unanchored && status == TransactionStatus.Success;
                info.MicroblockHash = response.result.microblock_hash;
            }

            return info;
        }

        private void UpdateStatus(TransactionStatus newStatus)
        {
            Status = newStatus;
        }

        static private TransferTransactionInfo TransferFrom(GetTransactionDetailsReponse.Transaction response)
        {
            return new TransferTransactionInfo { Memo = ExtractMemo(response.token_transfer.memo), Amount = response.token_transfer.amount };
        }

        static private ContractCallTransactionInfo ContractCallFrom(GetTransactionDetailsReponse.Transaction response)
        {
            var addressAndContract = response.contract_call.contract_id.Split('.');
            var arguments = response.contract_call.function_args.Select(x => Clarity.Value.FromHex(x.hex)).ToArray();
            Clarity.Value result = response.tx_result != null ? Clarity.Value.FromHex(response.tx_result.hex) : null;
            return new ContractCallTransactionInfo { Address = addressAndContract[0], Contract = addressAndContract[1], Function = response.contract_call.function_name, Result = result, Arguments = arguments};
        }

        static private string ExtractMemo(string hexedMemo)
        {
            var bytes = hexedMemo.ToHexByteArray().TrimEnd();
            var memo = Encoding.ASCII.GetString(bytes);
            return memo;
        }
    }

    public class ContractCallTransactionInfo: TransactionInfo
    {
        public string Address { get; set; }
        public string Contract { get; set; }
        public string Function { get; set; }

        public Clarity.Value Result { get; set; }

        public Clarity.Value[] Arguments { get; set; }

        public override string ToString()
        {
            string arguments = string.Join(", ", Arguments.Select(x => x.ToString()));
            return $"Call {Address}.{Contract}::{Function}({arguments}) {Status} {IsAnchored} => {Result}";
        }
    }

    public class TransferTransactionInfo : TransactionInfo
    {
        public string Memo { get; set; }
        public BigInteger Amount { get; set; }

        public override string ToString()
        {
            return $"Transfer({Memo}) {Amount} {Status} {IsAnchored} {MicroblockSequence}";
        }
    }
}

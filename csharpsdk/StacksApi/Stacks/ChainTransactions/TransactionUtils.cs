using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.ChainTransactions
{
    public static class TransactionUtils
    {
        static public async Task<AsyncCallResult<Transaction>> Prepare(this Blockchain chain, StacksAccountBase account, Transaction t)
        {
            var fee = t.Fee;
            var nonce = t.Nonce;

            if (fee == 0)
            {
                fee = await GetApproximateFee(chain, t).ConfigureAwait();
            }

            if (fee == 0)
            {
                fee = TransactionsManager.DefaultTransactionCostUstx;
            }

            if (nonce == 0)
            {
                var lastNonceResult = await chain.GetLastNonce(account.GetAddress(chain.GetAddressVersion())).ConfigureAwait();
                if (lastNonceResult.IsError)
                    return lastNonceResult.Error!;

                nonce = lastNonceResult.Data.possible_next_nonce;
            }

            t.UpdateFeeAndNonce(fee, nonce);

            return t;
        }

        static public Task<AsyncCallResult<string>> SignAndBroadcast(this Blockchain chain, StacksAccountBase account, Transaction transaction) {
            var ts = new TransactionSigner(transaction);
            ts.SignOrigin(account.PrivateKey);

            return chain.BroadcastRawTransaction(transaction.Serialize());
        }

        static public async Task<AsyncCallResult<string>> RunTransaction(this Blockchain chain, StacksAccountBase account, Transaction transaction)
        {
            var r = await Prepare(chain, account, transaction).ConfigureAwait();
            if (r.IsError)
                return r.Error!;

            return await SignAndBroadcast(chain, account, transaction).ConfigureAwait();
        }

        static public async Task<uint> GetApproximateFee(Blockchain chain, Transaction t)
        {
            var serialized = t.Serialize();
            var feeResults = await chain.GetTransactionApproximateFee(t.Payload.ToHexString(), (uint) serialized.Length).ConfigureAwait();
            if (feeResults.IsError)
                return 0;

            var fee = feeResults.Data.estimations[0].fee;
            return fee;
        }

        static public async Task<AsyncCallResult<List<TransactionInfo>>> GetTransactions(Blockchain chain, string address)
        {
            var result = await chain.GetAddressMempoolTransactions(address).ConfigureAwait();
            if (result.IsError)
                return result.Error!;

            List<TransactionInfo> infos = new List<TransactionInfo>();

            foreach (var t in result.Data.results)
            {
                var pending = TransactionInfo.FromData(chain, t);
                if (pending != null)
                    infos.Add(pending);
            }

            return infos;
        }

        static public async Task<AsyncCallResult<List<TransferTransactionInfo>>> GetStxTransactions(Blockchain chain, string address, bool incoming = true, bool outgoing = true)
        {
            var result = await GetTransactions(chain, address).ConfigureAwait();
            if (result.IsError)
                return result.Error!;

            List<TransferTransactionInfo> infos = new List<TransferTransactionInfo>();

            foreach (var t in result.Data)
            {
                if (t is TransferTransactionInfo transfer)
                {
                    if ((incoming && transfer.Recepient == address) || (outgoing && transfer.Sender == address))
                        infos.Add(transfer);
                }
            }

            return infos;
        }

        static public string TxIdToSimpleForm(string txId)
        {
            if (txId.StartsWith("0x"))
                return txId.Substring(2);
            return txId;
        }

        static public string TxIdToPrefixedForm(string txId)
        {
            if (txId.StartsWith("0x"))
                return txId;
            return "0x" + txId;
        }
    }
}

using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.ChainTransactions
{
    public static class TransactionUtils
    {
        static private ulong DEFAULT_COST = 1000;

        static public async Task<AsyncCallResult<Transaction>> Prepare(this Blockchain chain, StacksAccount account, Transaction t)
        {
            var fee = t.Fee;
            var nonce = t.Nonce;

            if (fee == 0)
            {
                fee = await GetApproximateFee(chain, t).ConfigureAwait(false);
            }

            if (fee == 0)
            {
                fee = DEFAULT_COST;
            }

            if (nonce == 0)
            {
                var lastNonceResult = await chain.GetLastNonce(account.GetAddress(chain.GetAddressVersion())).ConfigureAwait(false);
                if (lastNonceResult.IsError)
                    return new AsyncCallResult<Transaction>(lastNonceResult.Error!);

                nonce = lastNonceResult.Data.possible_next_nonce;
            }

            t.UpdateFeeAndNonce(fee, nonce);

            return new AsyncCallResult<Transaction>(t);
        }

        static public Task<AsyncCallResult<string>> SignAndBroadcast(this Blockchain chain, StacksAccount account, Transaction transaction) {
            var ts = new TransactionSigner(transaction);
            ts.SignOrigin(account.PrivateKey);

            return chain.BroadcastRawTransaction(transaction.Serialize());
        }

        static public async Task<AsyncCallResult<string>> RunTransaction(this Blockchain chain, StacksAccount account, Transaction transaction)
        {
            var r = await Prepare(chain, account, transaction).ConfigureAwait(false);
            if (r.IsError)
                return new AsyncCallResult<string>(r.Error);

            return await SignAndBroadcast(chain, account, transaction);
        }

        static public async Task<uint> GetApproximateFee(Blockchain chain, Transaction t)
        {
            var serialized = t.Serialize();
            var feeResults = await chain.GetTransactionApproximateFee(t.Payload.ToHexString(), (uint) serialized.Length).ConfigureAwait(false);
            if (feeResults.IsError)
                return 0;

            var fee = feeResults.Data.estimations[0].fee;
            return fee;
        }

        static public async Task<AsyncCallResult<List<TransactionInfo>>> GetTransactions(Blockchain chain, string address)
        {
            var result = await chain.GetAddressMempoolTransactions(address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<List<TransactionInfo>>(result.Error!);

            List<TransactionInfo> infos = new List<TransactionInfo>();

            foreach (var t in result.Data.results)
            {
                var pending = TransactionInfo.FromData(chain, t);
                if (pending != null)
                    infos.Add(pending);
            }

            return new AsyncCallResult<List<TransactionInfo>>(infos);
        }

        static public async Task<AsyncCallResult<List<TransferTransactionInfo>>> GetStxTransactions(Blockchain chain, string address, bool incoming = true, bool outgoing = true)
        {
            var result = await GetTransactions(chain, address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<List<TransferTransactionInfo>>(result.Error!);

            List<TransferTransactionInfo> infos = new List<TransferTransactionInfo>();

            foreach (var t in result.Data)
            {
                if (t is TransferTransactionInfo transfer)
                {
                    if ((incoming && transfer.Recepient == address) || (outgoing && transfer.Sender == address))
                        infos.Add(transfer);
                }
            }

            return new AsyncCallResult<List<TransferTransactionInfo>>(infos);
        }
    }
}

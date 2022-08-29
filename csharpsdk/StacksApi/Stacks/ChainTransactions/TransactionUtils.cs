using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.ChainTransactions
{
    public static class TransactionUtils
    {
        static private ulong DEFAULT_COST = 1000;

        static public async Task<AsyncCallResult<string>> RunTransaction(this Blockchain chain, StacksAccount account, Transaction t)
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
                nonce = lastNonceResult.Data.possible_next_nonce;
            }

            t.UpdateFeeAndNonce(fee, nonce);

            var ts = new TransactionSigner(t);
            ts.SignOrigin(account.PrivateKey);

            return await chain.BroadcastRawTransaction(t.Serialize()).ConfigureAwait(false);
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

        static public async Task<AsyncCallResult<List<TransferTransactionInfo>>> GetIncomingStxTransactions(Blockchain chain, string address)
        {
            var result = await chain.GetAddressMempoolTransactions(address).ConfigureAwait(false);
            if (result.IsError)
                return new AsyncCallResult<List<TransferTransactionInfo>>(result.Error!);


            List<TransferTransactionInfo> infos = new List<TransferTransactionInfo>();

            foreach (var t in result.Data.results)
            {
                var pending = TransactionInfo.FromData(chain, t);
                if (pending != null)
                {
                    if (pending is TransferTransactionInfo transfer)
                    {
                        if (transfer.Recepient == address)
                            infos.Add(transfer);
                    }
                }
            }

            return new AsyncCallResult<List<TransferTransactionInfo>>(infos);
        }
    }
}

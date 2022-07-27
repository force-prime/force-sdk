using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class Transactions
    {
        // https://docs.hiro.so/api#tag/Transactions/operation/get_transaction_list
        static public Task<AsyncCallResult<GetRecentTransactionsResponse>> GetRecentTransactions(this Blockchain chain, uint limit = 96, uint offset = 0, string[]? types = null, bool unanchored = false)
        {
            string methodName = chain.Endpoint + "extended/v1/tx";

            var requestData = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset },
                { "type", types },
                { "unanchored", unanchored },
            };

            return HttpAPIUtils.PerformHttpRequestJsonContent<GetRecentTransactionsResponse>(methodName, requestData, null);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/post_core_node_transactions
        static public Task<AsyncCallResult<string>> BroadcastRawTransaction(this Blockchain chain, byte[] rawData)
        {
            string methodName = chain.Endpoint + "v2/transactions";

            return HttpAPIUtils.PerformHttpRequestBinaryContent<string>(methodName, null, rawData);
        }

        // https://docs.hiro.so/api#tag/Fees/operation/get_fee_transfer
        static public Task<AsyncCallResult<GetStxTransferEstimatedFeeResponse>> GetStxTransferEstimatedFee(this Blockchain chain)
        {
            string methodName = chain.Endpoint + "v2/fees/transfer";

            return HttpAPIUtils.PerformHttpRequest<GetStxTransferEstimatedFeeResponse>(methodName);
        }

        //https://docs.hiro.so/api#tag/Fees/operation/post_fee_transaction
        static public Task<AsyncCallResult<GetTransactionApproximateFeeResponse>> GetTransactionApproximateFee(this Blockchain chain, string transactionPayload, uint estimatedLen = 0)
        {
            string methodName = chain.Endpoint + "v2/fees/transaction";
            var data = new
            {
                transaction_payload = transactionPayload,
                estimated_len = estimatedLen
            };
            return HttpAPIUtils.PerformHttpRequestJsonContent<GetTransactionApproximateFeeResponse>(methodName, null, data);
        }

        // https://docs.hiro.so/api#tag/Accounts/operation/get_single_transaction_with_transfers
        static public Task<AsyncCallResult<GetPrincipalTransactionDetailsReponse>> GetPrincipalTransactionDetails(this Blockchain chain, string principal, string txId)
        {
            string methodName = chain.Endpoint + $"extended/v1/address/{principal}/{txId}/with_transfers";

            return HttpAPIUtils.PerformHttpRequest<GetPrincipalTransactionDetailsReponse>(methodName, null, null);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/get_tx_list_details
        static public Task<AsyncCallResult<GetTransactionDetailsReponse>> GetTransactionsDetails(this Blockchain chain, string[] txIds, uint eventOffset = 0, uint eventLimit = 96, bool unanchored = false)
        {
            string methodName = chain.Endpoint + "extended/v1/tx/multiple";

            var requestData = new Dictionary<string, object?> {
                { "tx_id", txIds },
                { "event_offset", eventOffset },
                { "event_limit", eventLimit },
                { "unanchored", unanchored },
            };

            return HttpAPIUtils.PerformHttpRequest<GetTransactionDetailsReponse>(methodName, requestData, null);
        }

        public class GetTransactionDetailsReponse : Dictionary<string, GetTransactionDetailsReponse.Result>
        {
            public class Result
            {
                public bool found;
                public Transaction result;
            }

            public class Transaction
            {
                public string tx_id;
                public uint nonce;
                public string tx_status;
                public string tx_type;
                public bool is_unanchored;
                public string microblock_hash;
                public uint microblock_sequence;
                public TokenTransfer token_transfer;
                public ContractCall contract_call;
                public TxResult tx_result;

                public class TxResult
                {
                    public string hex;
                }

                public class TokenTransfer {
                    public string recipient_address;
                    public ulong amount;
                    public string memo;
                }

                public class ContractCall
                {
                    public string contract_id;
                    public string function_name;
                    public string function_signature;
                    public Argument[] function_args;

                    public class Argument
                    {
                        public string hex;
                        public string name;
                        public string type;
                    }
                }
            }
        }

        public class GetPrincipalTransactionDetailsReponse
        {

        }

        public class GetTransactionApproximateFeeResponse {
            public double cost_scalar_change_by_byte;
            public EstimatedCost estimated_cost;
            public Estimation[] estimations;

            public class Estimation
            {
                public double fee_rate;
                public uint fee;
            }

            public class EstimatedCost
            {
                public uint read_count;
                public uint read_length;
                public uint runtime;
                public uint write_count;
                public uint write_length;
            }
        }

        public class GetStxTransferEstimatedFeeResponse
        {
            public uint fee_rate;
        }


        public class BroadcastRawTranasctionResponse
        {

        }

        public class GetRecentTransactionsResponse
        {

        }
    }
}

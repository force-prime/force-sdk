using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class Transactions
    {
        // https://docs.hiro.so/api#tag/Transactions/operation/get_address_mempool_transactions
        static public Task<AsyncCallResult<GetMempoolTransactionsResponse>> GetAddressMempoolTransactions(this Blockchain chain, string address, uint limit = 20, uint offset = 0)
        {
            string methodName = chain.Endpoint + $"extended/v1/address/{address}/mempool";

            var requestData = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset },
            };

            return HttpAPIUtils.GetRequest<GetMempoolTransactionsResponse>(methodName, requestData);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/get_mempool_transaction_list
        static public Task<AsyncCallResult<GetMempoolTransactionsResponse>> GetMempoolTransactions(this Blockchain chain, string senderAddress, string? recepientAddress = null, string? address = null, uint limit = 20, uint offset = 0)
        {
            string methodName = chain.Endpoint + "extended/v1/tx/mempool";

            var requestData = new Dictionary<string, object?> {
                { "sender_address", senderAddress },
                { "recipient_address", recepientAddress },
                { "address", address },
                { "limit", limit },
                { "offset", offset },
            };

            return HttpAPIUtils.GetRequest<GetMempoolTransactionsResponse>(methodName, requestData);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/get_transaction_list
        static public Task<AsyncCallResult<GetRecentTransactionsResponse>> GetRecentTransactions(this Blockchain chain, uint limit = 20, uint offset = 0, string[]? types = null, bool unanchored = false)
        {
            string methodName = chain.Endpoint + "extended/v1/tx";

            var requestData = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset },
                { "type", types },
                { "unanchored", unanchored },
            };

            return HttpAPIUtils.GetRequest<GetRecentTransactionsResponse>(methodName, requestData);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/post_core_node_transactions
        static public Task<AsyncCallResult<string>> BroadcastRawTransaction(this Blockchain chain, byte[] rawData)
        {
            string methodName = chain.Endpoint + "v2/transactions";

            return HttpAPIUtils.PostBinary<string>(methodName, null, rawData);
        }

        // https://docs.hiro.so/api#tag/Fees/operation/get_fee_transfer
        static public Task<AsyncCallResult<ulong>> GetStxTransferEstimatedFee(this Blockchain chain)
        {
            string methodName = chain.Endpoint + "v2/fees/transfer";

            return HttpAPIUtils.GetRequest<ulong>(methodName);
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
            return HttpAPIUtils.PostJson<GetTransactionApproximateFeeResponse>(methodName, null, data);
        }

        // https://docs.hiro.so/api#tag/Accounts/operation/get_single_transaction_with_transfers
        static public Task<AsyncCallResult<GetPrincipalTransactionDetailsReponse>> GetPrincipalTransactionDetails(this Blockchain chain, string principal, string txId)
        {
            string methodName = chain.Endpoint + $"extended/v1/address/{principal}/{txId}/with_transfers";

            return HttpAPIUtils.GetRequest<GetPrincipalTransactionDetailsReponse>(methodName);
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

            return HttpAPIUtils.GetRequest<GetTransactionDetailsReponse>(methodName, requestData);
        }

        // https://docs.hiro.so/api#tag/Transactions/operation/get_filtered_events
        static public Task<AsyncCallResult<GetTransactionEventsResponse>> GetTransactionEvents(this Blockchain chain, string txId, int limit = 20, int offset = 0)
        {
            string methodName = chain.Endpoint + "extended/v1/tx/events";

            var requestData = new Dictionary<string, object?> {
                { "tx_id", txId },
                { "offset", offset },
                { "limit", limit },
            };

            return HttpAPIUtils.GetRequest<GetTransactionEventsResponse>(methodName, requestData);
        }

        public class GetTransactionDetailsReponse : Dictionary<string, GetTransactionDetailsReponse.Result>
        {
            public class Result
            {
                public bool found;
                public TransactionData result;
            }
        }

        public class GetTransactionEventsResponse
        {
            public TransactionData.Event[] events;
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
            public TransactionData[] results;
        }

        public class GetMempoolTransactionsResponse
        {
            public int total;
            public TransactionData[] results;
        }

        public class TransactionData
        {
            public string tx_id;
            public uint nonce;
            public string tx_status;
            public string tx_type;
            public ulong fee_rate;
            public bool is_unanchored;
            public long burn_block_time;
            public string sender_address;
            public string microblock_hash;
            public uint microblock_sequence;
            public TokenTransfer token_transfer;
            public ContractCall contract_call;
            public SmartContract smart_contract;
            public TxResult tx_result;
            public uint event_count;
            public Event[] events;

            public class Event
            {
                public string event_type;
                public TransactionEventAsset asset;
                public ContractLog contract_log;
            }

            public class ContractLog
            {
                public Value value;
                public class Value
                {
                    public string hex;
                }
            }

            public class TransactionEventAsset
            {
                public string asset_event_type;
                public string asset_id;
                public ulong amount;
            }

            public class TxResult
            {
                public string hex;
            }

            public class SmartContract
            {
                public string contract_id;
            }

            public class TokenTransfer
            {
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
}

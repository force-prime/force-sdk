using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class Accounts
    {
        // https://docs.hiro.so/api#tag/Accounts/operation/get_account_stx_balance
        static public Task<AsyncCallResult<GetSTXBalanceResponse>> GetSTXBalance(this Blockchain chain, string principal, bool unanchored = false, uint? until_block = null)
        {
            string methodName = $"{chain.Endpoint}extended/v1/address/{principal}/stx";

            var getFields = new Dictionary<string, object?> {
                { "unanchored", unanchored },
                { "until_block", until_block }
            };

            return HttpAPIUtils.GetRequest<GetSTXBalanceResponse>(methodName, getFields);
        }

        // https://docs.hiro.so/api#tag/Accounts/operation/get_account_balance
        static public Task<AsyncCallResult<GetBalancesResponse>> GetBalances(this Blockchain chain, string principal, bool unanchored = false, uint? until_block = null)
        {
            string methodName = $"{chain.Endpoint}extended/v1/address/{principal}/balances";

            var getFields = new Dictionary<string, object?> {
                { "unanchored", unanchored },
                { "until_block", until_block }
            };

            return HttpAPIUtils.GetRequest<GetBalancesResponse>(methodName, getFields);
        }

        // https://docs.hiro.so/api#tag/Accounts/operation/get_account_nonces
        static public Task<AsyncCallResult<GetLastNonceResponse>> GetLastNonce(this Blockchain chain, string address)
        {
            string methodName = $"{chain.Endpoint}extended/v1/address/{address}/nonces";

            return HttpAPIUtils.GetRequest<GetLastNonceResponse>(methodName);
        }

        // https://docs.hiro.so/api#tag/Accounts/operation/get_account_inbound
        static public Task<AsyncCallResult<GetInboundStxTransfersResponse>> GetInboundStxTransfers(this Blockchain chain, string address, int limit = 30, int offset = 0, uint? height = null, uint? until_block = null, bool unanchored = false)
        {
            string methodName = $"{chain.Endpoint}extended/v1/address/{address}/stx_inbound";
            var getFields = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset },
                { "height", height },
                { "unanchored", unanchored },
                { "until_block", until_block }
            };

            return HttpAPIUtils.GetRequest<GetInboundStxTransfersResponse>(methodName, getFields);
        }

        public class GetInboundStxTransfersResponse : HttpAPIUtils.JsonDataBase
        {
            public long total;
            public InboundStxTransfer[] results;

            public class InboundStxTransfer
            {
                public string sender;
                public ulong amount;
                public string memo;
                public uint block_height;
                public string tx_id;
                public string transfer_type;
                public uint tx_index;
            }
        }

        public class GetLastNonceResponse : HttpAPIUtils.JsonDataBase
        {
            public uint? last_mempool_tx_nonce;
            public uint? last_executed_tx_nonce;
            public uint possible_next_nonce;
            public uint[] detected_missing_nonces;
        }

        public class GetSTXBalanceResponse : HttpAPIUtils.JsonDataBase
        {
            public ulong balance;
            public ulong total_sent;
            public ulong total_received;
            public ulong locked;
            public ulong lock_height;
        }

        public class GetBalancesResponse
        {
            public StxInfo stx;
            public Dictionary<string, NFTInfo> non_fungible_tokens;
            public Dictionary<string, FTInfo> fungible_tokens;
            /*
            public ulong total_sent;
            public ulong total_received;
            public ulong locked;
            public ulong lock_height;
            */

            public class StxInfo
            {
                public ulong balance;
            }

            public class FTInfo
            {
                public ulong balance;
            }

            public class NFTInfo
            {
                public ulong count;
            }
        }
    }
}

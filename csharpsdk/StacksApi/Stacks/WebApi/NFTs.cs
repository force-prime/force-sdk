using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class NFTs
    {
        // https://docs.hiro.so/api#tag/Non-Fungible-Tokens/operation/get_nft_holdings
        static public Task<AsyncCallResult<GetNFTHoldingsResponse>> GetNFTHoldings(this Blockchain chain, string principal, string[]? nftTypes = null, bool unanchored = false, ulong limit = 50, ulong offset = 0)
        {
            string methodName = $"{chain.Endpoint}extended/v1/tokens/nft/holdings";

            var getFields = new Dictionary<string, object?> {
                { "principal", principal },
                { "asset_identifiers", nftTypes },
                { "unanchored", unanchored },
                { "limit", limit },
                { "offset", offset },
            };

            return HttpAPIUtils.PerformHttpRequestJsonContent<GetNFTHoldingsResponse>(methodName, getFields, null);
        }

        // https://docs.hiro.so/api#tag/Non-Fungible-Tokens/operation/get_contract_nft_metadata
        static public Task<AsyncCallResult<GetNFTContractMetadataResponse>> GetNFTContractMetadata(this Blockchain chain, string tokenContractId)
        {
            string methodName = $"{chain.Endpoint}extended/v1/tokens/{tokenContractId}/nft/metadata";

            return HttpAPIUtils.PerformHttpRequestJsonContent<GetNFTContractMetadataResponse>(methodName, null, null);
        }

        public class GetNFTContractMetadataResponse : HttpAPIUtils.JsonDataBase
        {
            public string token_uri;
            public string name;
            public string description;
            public string image_uri;
            public string image_canonical_uri;
            public string tx_id;
            public string sender_address;
        }

        public class GetNFTHoldingsResponse : HttpAPIUtils.JsonDataBase
        {
            public NFTData[] results;
            /*
            public ulong total_sent;
            public ulong total_received;
            public ulong locked;
            public ulong lock_height;
            */

            public class NFTData
            {
                public string asset_identifier;
                public Value value;
                public class Value
                {
                    public string hex;
                    public string repr;
                }

                public (Clarity.Value id, string address, string contract, string nft) Extract()
                {
                    var f = Address.ParseFromFullTokenId(asset_identifier);
                    var id = Clarity.Value.FromHex(value.hex);
                    return (id, f.address, f.contract, f.token);
                }
            }
        }
    }
}

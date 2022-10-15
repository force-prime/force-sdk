using StacksForce.Utils;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    public static class FungibleTokens
    {
        // https://docs.hiro.so/api#tag/Fungible-Tokens/operation/get_contract_ft_metadata
        static public Task<AsyncCallResult<GetFungibleTokenMetadataResponse>> GetFungibleTokenMetadata(this Blockchain chain, string tokenContractId)
        {
            string methodName = $"{chain.Endpoint}extended/v1/tokens/{tokenContractId}/ft/metadata";

            return HttpAPIUtils.PostJson<GetFungibleTokenMetadataResponse>(methodName, null, null);
        }

        public class GetFungibleTokenMetadataResponse
        {
            public string token_uri;
            public string name;
            public string description;
            public string image_uri;
            public string image_canonical_uri;
            public string symbol;
            public uint decimals;
            public string tx_id;
            public string sender_address;
        }
    }
}

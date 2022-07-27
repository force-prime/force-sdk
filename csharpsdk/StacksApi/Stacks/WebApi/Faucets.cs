using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class Faucets
    {
        // https://docs.hiro.so/api/#tag/Faucets/operation/run_faucet_stx
        static public async Task<AsyncCallResult<string>> GetSTXTestnetTokens(this Blockchain chain, string address)
        {
            string methodName = $"{chain.Endpoint}extended/v1/faucets/stx";

            var getFields = new Dictionary<string, object?> {
                { "address", address },
            };

            var result = await HttpAPIUtils.PerformHttpRequestJsonContent<GetSTXTestnetTokensResult>(methodName, getFields, new { });
            if (result.IsError)
                return new AsyncCallResult<string>(result.Error!);
            return new AsyncCallResult<string>(result.Data.txId);
        }

        private class GetSTXTestnetTokensResult
        {
            public bool success;
            public string txId;
        }
    }
}

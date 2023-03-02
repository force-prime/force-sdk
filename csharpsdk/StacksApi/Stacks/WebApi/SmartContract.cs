using StacksForce.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class SmartContracts
    {
        private const string CONTRACTS_ENDPOINT = "v2/contracts";

        private static readonly string[] EMPTY_STRING_ARRAY = new string[0];

        // https://docs.hiro.so/api#tag/Smart-Contracts/operation/call_read_only_function
        static public async Task<AsyncCallResult<Clarity.Value>> CallReadOnly(this Blockchain chain, string contractAddress, string contractName, string functionName, string txSender, params Clarity.Value[] arguments)
        {
            string methodName = $"{chain.Endpoint}{CONTRACTS_ENDPOINT}/call-read/{contractAddress}/{contractName}/{functionName}";

            var request = new CallReadOnlyRequest { sender = txSender, arguments = arguments.Select(x => x.AsHex()).ToArray() ?? EMPTY_STRING_ARRAY };

            var result = await HttpAPIUtils.PostJson<CallReadOnlyResponse>(methodName, null, request);
            if (result.IsSuccess && !result.Data!.okay)
                return new Error("error", result.Data.cause);

            if (result.IsSuccess)
                return Clarity.Value.FromHex(result.Data!.result);

            return result.Error!;
        }

        private class CallReadOnlyRequest
        {
            public string sender;
            public string[] arguments;
        }

        private class CallReadOnlyResponse
        {
            public bool okay;
            public string cause;
            public string result;
        }
    }
}

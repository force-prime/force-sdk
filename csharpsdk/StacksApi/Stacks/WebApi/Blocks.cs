using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    static public class Blocks
    {
        // https://docs.hiro.so/api#tag/Microblocks/operation/get_microblock_list
        static public Task<AsyncCallResult<GetRecentMicroblocksResponse>> GetRecentMicroblocks(this Blockchain chain, uint limit = 20, uint offset = 0)
        {
            string methodName = $"{chain.Endpoint}extended/v1/microblock";

            var getFields = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset }
            };

            return HttpAPIUtils.PerformHttpRequest<GetRecentMicroblocksResponse>(methodName, getFields, null);
        }

        public class GetRecentMicroblocksResponse
        {
            public Microblock[] results;

            public class Microblock
            {
                public string microblock_hash;
                public string microblock_parent_hash;
                public string block_hash;
                public uint microblock_sequence;
            }
        }
    }
}

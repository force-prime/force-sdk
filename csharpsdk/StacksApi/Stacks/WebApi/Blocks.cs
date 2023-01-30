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

            return HttpAPIUtils.GetRequest<GetRecentMicroblocksResponse>(methodName, getFields);
        }

        //https://docs.hiro.so/api#tag/Blocks/operation/get_block_list
        static public Task<AsyncCallResult<GetRecentBlocksRepsonse>> GetRecentBlocks(this Blockchain chain, int limit, int offset)
        {
            string methodName = $"{chain.Endpoint}extended/v1/block";

            var getFields = new Dictionary<string, object?> {
                { "limit", limit },
                { "offset", offset }
            };

            return HttpAPIUtils.GetRequest<GetRecentBlocksRepsonse>(methodName, getFields);
        }


        // https://docs.hiro.so/api#tag/Blocks/operation/get_block_by_height
        static public Task<AsyncCallResult<GetBlockByHeightResponse>> GetBlockByHeight(this Blockchain chain, uint height)
        {
            string methodName = $"{chain.Endpoint}extended/v1/block/by_height/{height}";

            return HttpAPIUtils.GetRequest<GetBlockByHeightResponse>(methodName);
        }

        public class GetRecentBlocksRepsonse
        {
            public Block[] results;
        }

        public class GetBlockByHeightResponse : Block
        {

        }

        public class Block
        {
            public bool canonical;
            public uint height;
            public string[] txs;
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

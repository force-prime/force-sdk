using StacksForce.Stacks.WebApi;
using StacksForce.Stacks;
using StacksForce.Stacks.Metadata;
using System.Threading.Tasks;

namespace ChainAbstractions.Stacks
{
    internal static class NFTUtils
    {
        static public async Task<NFTMetaData?> GetMetaData(string address, string contract, Clarity.Value id)
        {
            var chain = StacksAbstractions.FromAddress(address);
            var uri = await chain.AsStacksBlockchain().ReadonlyGetString(address, contract, "get-token-uri", id).ConfigureAwait(false);
            if (uri.IsSuccess)
            {
                var uriStr = uri.Data;
                if (!string.IsNullOrEmpty(uriStr))
                {
                    if (uriStr.Contains("{id}"))
                    {
                        uriStr = uriStr.Replace("{id}", id.ToString());
                    }
                    return await NFTMetaData.FromUrl(uriStr).ConfigureAwait(false);
                }
            }
            return null;
        }

        static public async Task<INFT> GetFrom(string address, string contract, string nft, Clarity.Value id)
        {
            NFTMetaData? metaData = await GetMetaData(address, contract, id).ConfigureAwait(false);

            var name = nft;
            var description = string.Empty;
            var image = string.Empty;


            if (metaData != null)
            {
                if (!string.IsNullOrEmpty(metaData.Name))
                    name = metaData.Name;

                if (!string.IsNullOrEmpty(metaData.Description))
                    description = metaData.Description;

                if (!string.IsNullOrEmpty(metaData.Image))
                    image = metaData.Image;
            }

            return new NFT(name, description, image);
        }

        public class NFT : INFT
        {
            public string Description { get; }
            public string ImageUrl { get; }
            public string Name { get; }

            public NFT(string name, string description, string imageUrl)
            {
                Name = name;
                Description = description;
                ImageUrl = imageUrl;
            }
        }
    }
}

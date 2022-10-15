using StacksForce.Stacks.WebApi;
using StacksForce.Stacks;
using StacksForce.Stacks.Metadata;
using System.Threading.Tasks;
using StacksForce.Utils;

namespace ChainAbstractions.Stacks
{
    internal static class NFTUtils
    {
        static public async Task<NFTMetaData?> GetMetaData(string address, string contract, Clarity.Value id)
        {
            var chain = StacksAbstractions.FromAddress(address);
            var uri = await chain.AsStacksBlockchain().ReadonlyGetString(address, contract, "get-token-uri", id).ConfigureAwait();
            if (uri.IsSuccess)
            {
                var uriStr = uri.Data;
                if (!string.IsNullOrEmpty(uriStr))
                {
                    if (uriStr.Contains("{id}"))
                    {
                        uriStr = uriStr.Replace("{id}", id.ToString());
                    }
                    return await NFTMetaData.FromUrl(uriStr).ConfigureAwait();
                }
            }
            return null;
        }

        static public async Task<INFT> GetFrom(string address, string contract, string nft, Clarity.Value id)
        {
            NFTMetaData? metaData = await GetMetaData(address, contract, id).ConfigureAwait();

            var name = nft;
            var description = string.Empty;
            var image = string.Empty;

            var assetTypeId = $"{address}.{contract}::{nft}";
            var result = new NFT(assetTypeId, id, name, description, image);

            if (metaData != null)
                result.UpdateFromMetaData(metaData);

            return result;
        }

        public class NFT : INFT
        {
            public string AssetTypeId { get; }
            public Clarity.Value Id { get; }

            public string Description { get; private set; }
            public string ImageUrl { get; private set; }
            public string Name { get; private set; }

            public NFT(string assetTypeId, Clarity.Value id, string name, string description, string imageUrl)
            {
                AssetTypeId = assetTypeId;
                Id = id;
                Name = name;
                Description = description;
                ImageUrl = imageUrl;
            }

            public void UpdateFromMetaData(NFTMetaData metaData)
            {
                if (!string.IsNullOrEmpty(metaData.Name))
                    Name = metaData.Name;

                if (!string.IsNullOrEmpty(metaData.Description))
                    Description = metaData.Description;

                if (!string.IsNullOrEmpty(metaData.Image))
                    ImageUrl = metaData.Image;
            }

            public async Task<NFTMetaData?> GetMetaData()
            {
                var typeId = Address.ParseFromFullTokenId(AssetTypeId);
                var metaData = await NFTUtils.GetMetaData(typeId.address, typeId.contract, Id).ConfigureAwait();
                if (metaData != null)
                    UpdateFromMetaData(metaData);
                return metaData;
            }
        }
    }
}

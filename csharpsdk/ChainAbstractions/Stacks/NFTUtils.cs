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
            var chain = StacksAbstractions.FromAddress(address).AsStacksBlockchain();
            var r = await NFTMetaData.FromContractAndId(chain, $"{address}.{contract}", id.ToString()).ConfigureAwait();
            if (r.IsSuccess)
                return r;
            var uri = await GetNftMetaUrl(chain, address, contract, id).ConfigureAwait();
            if (string.IsNullOrEmpty(uri))
                return null;
            return await NFTMetaData.FromUrl(uri).ConfigureAwait();
        }

        static public async Task<INFT> GetFrom(string address, string contract, string nft, Clarity.Value id)
        {
            var assetTypeId = $"{address}.{contract}::{nft}";

            var result = new NFT(assetTypeId, id, nft, string.Empty, string.Empty, string.Empty);

            NFTMetaData? metaData = await GetMetaData(address, contract, id).ConfigureAwait();
            if (metaData != null)
                result.UpdateFromMetaData(metaData);

            return result;
        }

        static private async Task<string?> GetNftMetaUrl(Blockchain chain, string address, string contract, Clarity.Value id)
        {
            var uri = await chain.ReadonlyGetString(address, contract, "get-token-uri", id).ConfigureAwait();
            if (uri.IsSuccess)
            {
                var uriStr = uri.Data;
                if (!string.IsNullOrEmpty(uriStr))
                {
                    if (uriStr.Contains("{id}"))
                    {
                        uriStr = uriStr.Replace("{id}", id.ToString());
                    }
                    return uriStr;
                }
            }
            return null;
        }

        public class NFT : INFT
        {
            public string AssetTypeId { get; }
            public Clarity.Value Id { get; }

            public string Description { get; private set; }
            public string ImageUrl { get; private set; }
            public string Name { get; private set; }

            public string Collection { get; private set; }

            string INFT.Id => $"{AssetTypeId}::{Id.ToHexString()}";

            public NFT(string assetTypeId, Clarity.Value id, string name, string description, string imageUrl, string collection)
            {
                AssetTypeId = assetTypeId;
                Id = id;
                Name = name;
                Description = description;
                ImageUrl = imageUrl;
                Collection = collection;
            }

            public void UpdateFromMetaData(NFTMetaData metaData)
            {
                if (!string.IsNullOrEmpty(metaData.Name))
                    Name = metaData.Name;

                if (!string.IsNullOrEmpty(metaData.Description))
                    Description = metaData.Description;

                if (!string.IsNullOrEmpty(metaData.Image))
                    ImageUrl = metaData.Image;

                if (!string.IsNullOrEmpty(metaData.Collection))
                    Collection = metaData.Collection;
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

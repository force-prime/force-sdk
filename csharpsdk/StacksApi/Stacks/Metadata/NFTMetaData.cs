using StacksForce.Utils;
using System;
using System.Threading.Tasks;

namespace StacksForce.Stacks.Metadata
{
    public class NFTMetaData
    {
        private const string NFT_SERVICE_URL = "metadata/v1/nft";

        private static readonly NFTMetaData EMPTY = new NFTMetaData();

        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Image { get; private set; }
        public string? Collection { get; private set; }

        static private NFTMetaData FromJson(JsonFormat data)
        {
            try
            {
                string collection = data.properties?.collection_name;
                if (string.IsNullOrEmpty(collection))
                    collection = data.properties?.collection;
                if (string.IsNullOrEmpty(collection))
                    collection = data.properties?.collectionName;

                string description = data.description;
                string image = data.image;
                if (!string.IsNullOrEmpty(data.cached_image))
                    image = data.cached_image;
                if (string.IsNullOrEmpty(description))
                    description = data.properties?.description;
                return new NFTMetaData { Name = data.name, Description = description, Image = image, Collection = collection };
            } catch (Exception e)
            {
                Log.Debug("NFTMetaData.FromJson failed: " + e);
                return EMPTY;
            }
        }

        static public async Task<AsyncCallResult<NFTMetaData>> FromContractAndId(Blockchain chain, string contract, string id)
        {
            string url = $"{chain.Endpoint}{NFT_SERVICE_URL}/{contract}/{id}";
            var r = await Dependencies.DependencyProvider.HttpClient.Get(url).ConfigureAwait();
            if (r.IsSuccess)
            {
                try
                {
                    var data = JsonService.Deserialize<HiroJsonFormat>(r.Data)!;
                    if (data.metadata != null)
                        return FromJson(data.metadata);
                }
                catch { }
                return EMPTY;
            }
            return r.Error!;
        }

        static public async Task<NFTMetaData> FromUrl(string url)
        {
            url = HttpHelper.GetHttpUrlFrom(url);
            var r = await Dependencies.DependencyProvider.HttpClient.Get(url).ConfigureAwait();
            if (r.IsSuccess)
            {
                try
                {
                    var data = JsonService.Deserialize<JsonFormat>(r.Data)!;
                    return FromJson(data);
                }
                catch { }
            }
            return EMPTY;
        }

        private class HiroJsonFormat
        {
            public string token_uri;
            public JsonFormat metadata;
        }

        private class JsonFormat
        {
            public string name;
            public string image;
            public string cached_image;
            public string description;
            public Properties properties;
            public class Properties
            {
                public string description;
                public string collection_name;
                public string collectionName;
                public string collection;
            }
        }

        public override string ToString()
        {
            return $"NFT: {Name} {Description} {Image}";
        }
    }

}

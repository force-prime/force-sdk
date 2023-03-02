using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Threading.Tasks;

namespace StacksForce.Stacks.Metadata
{
    public class NFTMetaData
    {
        private static readonly NFTMetaData Empty = new NFTMetaData();

        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Image { get; private set; }

        static public NFTMetaData FromJson(string? json)
        {
            try
            {
                var data = JsonService.Deserialize<JsonFormat>(json)!;
                string description = data.description;
                if (string.IsNullOrEmpty(description))
                    description = data.properties?.description;
                return new NFTMetaData { Name = data.name, Description = description, Image = data.image };

            } catch (Exception e)
            {
                Log.Debug("NFTMetaData.FromJson failed: " + e);
                return Empty;
            }
        }

        static public async Task<NFTMetaData> FromUrl(string url)
        {
            url = HttpHelper.GetHttpUrlFrom(url);
            var r = await Dependencies.DependencyProvider.HttpClient.Get(url).ConfigureAwait();
            if (r.IsSuccess)
                return FromJson(r.Data);
            else
                return Empty;
        }

        private class JsonFormat
        {
            public string name;
            public string image;
            public string description;
            public string collection;
            public Properties properties;
            public class Properties
            {
                public string description;
            }
        }

        public override string ToString()
        {
            return $"NFT: {Name} {Description} {Image}";
        }
    }

}

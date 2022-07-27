using StacksForce.Utils;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace StacksForce.Stacks.Metadata
{
    public class NFTMetaData
    {
        private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions { IncludeFields = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };

        private static readonly NFTMetaData Empty = new NFTMetaData();

        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Image { get; private set; }

        static public NFTMetaData FromJson(string? json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonFormat>(json, SERIALIZER_OPTIONS)!;
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
            string? data = null;
            url = HttpHelper.GetHttpUrlFrom(url);
            var r = await HttpHelper.SendRequest(url);
            if (r.IsSuccess)
                data = r.Data;
            else
                return Empty;

            return FromJson(data);
        }

        private class JsonFormat
        {
            public string name;
            public string image;
            public string description;
            public string collection;
            public object[] attributes;
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

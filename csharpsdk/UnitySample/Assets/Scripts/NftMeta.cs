using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

static public class NftMeta 
{
    static private readonly CachedDictionaryAsync<INFT, bool> _nftsMeta = new CachedDictionaryAsync<INFT, bool>(UpdateMeta);
    static private readonly CachedDictionaryAsync<string, Sprite> _nftImage = new CachedDictionaryAsync<string, Sprite>(GetImage);

    static public ValueTask<bool> GetNft(INFT nft) => _nftsMeta.Get(nft);
    static public ValueTask<Sprite> GetImage(string uri) => _nftImage.Get(uri);

    private static async Task<bool> UpdateMeta(INFT nft, object data)
    {
        var r = await nft.RetrieveMetaData();
        return r != null;
    }

    private static async Task<Sprite> GetImage(string uri, object data)
    {
        uri = HttpHelper.GetHttpUrlFrom(uri);
        var request = UnityWebRequestTexture.GetTexture(uri);
        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
                return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        return null;
    }
}

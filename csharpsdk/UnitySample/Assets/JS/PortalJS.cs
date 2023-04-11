using System.Runtime.InteropServices;

static public class PortalJS
{
    [DllImport("__Internal")]
    private static extern void RequestMemorableNFT(int score, string token);

    static public void SendNftMintRequest(int score, string token)
    {
        RequestMemorableNFT(score, token);
    }

    static public void SendComplete(int score, string token)
    {
       
    }
}

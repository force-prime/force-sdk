using ChainAbstractions.Stacks;
using StacksForce.Utils;
using System;
using System.Web;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    static public string Token => _token;
    static private string _token;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StacksForce.ForceSDK.Init();
    }

    private void Start()
    {
        if (ExtractPassedData(out var address, out _token))
            Game.Current.Login(StacksAbstractions.FromAddress(address).GetWalletInfoForAddress(address));
    }

    private bool ExtractPassedData(out string address, out string token)
    {
        address = null;
        token = null;
        address = "SP136AXDAQ41R31GJWJX8KX14E2T4K8PA08NCE6Q5";
        return true;

        if (string.IsNullOrEmpty(Application.absoluteURL))
            return false;

        try
        {
            Uri myUri = new Uri(Application.absoluteURL);
            var getParams = HttpUtility.ParseQueryString(myUri.Query);

            address = getParams.Get("address");
            token = getParams.Get("token");

            return !string.IsNullOrEmpty(address);
        }
        catch (Exception)
        {
            Log.Fatal("Can't parse url: " + Application.absoluteURL);
            return false;
        }
    }
}

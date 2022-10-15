namespace StacksForce.Dependencies
{
    static public class DependencyProvider
    {
        static public IBIP39 BIP39 { get; set; }
        static public IHDKeyProvider HDKey { get; set; }
        static public ICryptography Cryptography { get; set; }
        static public IHttpClient HttpClient { get; set; }
    }
}

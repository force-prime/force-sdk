namespace StacksForce.Dependencies
{
    static public class DependencyProvider
    {
        static public IBtcFeatures BtcFeatures { get; set; }
        static public ICryptography Cryptography { get; set; }
        static public IHttpClient HttpClient { get; set; }
    }
}

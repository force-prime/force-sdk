using Cryptography.ECDSA;
using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;
using StacksForce.Dependencies;
using StacksForce.Stacks.WebApi;
using StacksForce.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace StacksForce.Stacks
{
    static public class StacksDependencies
    {
        static public void SetupDefault()
        {
            if (DependencyProvider.Cryptography == null)
                DependencyProvider.Cryptography = new DefaultCryptography();
            if (DependencyProvider.HDKey == null)
                DependencyProvider.HDKey = new DefaultHDKey();
            if (DependencyProvider.BIP39 == null)
                DependencyProvider.BIP39 = new DefaultBIP39();
            if (DependencyProvider.HttpClient == null)
                DependencyProvider.HttpClient = new DefaultHttpClient();
        }

        private class DefaultHttpClient : IHttpClient
        {
            private static readonly HttpHelper.IRetryStrategy DEFAULT_RETRY_STRATEGY = new HttpHelper.NRetryStrategy(0, 3000);

            private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = HttpAPIUtils.SERIALIZER_OPTIONS;

            public Task<AsyncCallResult<string>> Get(string uri)
            {
                return HttpHelper.SendRequest(uri, null, DEFAULT_RETRY_STRATEGY);
            }

            public Task<AsyncCallResult<string>> Post(string uri)
            {
                return PostJson(uri, new { }); // force post
            }

            public Task<AsyncCallResult<string>> PostBinary(string uri, byte[] bytes)
            {
                var content = new ByteArrayContent(bytes);
                content.Headers.Clear();
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return HttpHelper.SendRequest(uri, content, DEFAULT_RETRY_STRATEGY);
            }

            public Task<AsyncCallResult<string>> PostJson(string uri, object json)
            {
                var content = JsonContent.Create(json, new MediaTypeHeaderValue("application/json"), SERIALIZER_OPTIONS);
                return HttpHelper.SendRequest(uri, content, DEFAULT_RETRY_STRATEGY);
            }
        }

        private class DefaultCryptography : ICryptography
        {
            public byte[] RipeMD160(byte[] data)
            {
                var digest = new RipeMD160Digest();
                digest.BlockUpdate(data, 0, data.Length);
                var outArray = new byte[20];
                digest.DoFinal(outArray, 0);
                return outArray;
            }

            public byte[] Secp256k1GetPublicKey(byte[] privateKey, bool compressed)
            {
                return Secp256K1Manager.GetPublicKey(privateKey, compressed);
            }

            public byte[] Secp256k1Sign(byte[] data, byte[] privateKey, out int recoveryId)
            {
                var sig = Secp256K1Manager.SignCompact(data, privateKey, out recoveryId);
                return sig;
            }

            public byte[] Sha256(byte[] data)
            {
                using (SHA256 sha = SHA256.Create())
                    return sha.ComputeHash(data);
            }

            public byte[] Sha512_256(byte[] data)
            {
                SHA512_256 sha = new SHA512_256();
                var result = new byte[sha.GetDigestSize()];

                sha.BlockUpdate(data, 0, data.Length);
                sha.DoFinal(result, 0);

                return result;
            }
        }

        private class HDKey : Dependencies.IHDKey
        {
            private ExtKey _extKey;

            public HDKey(ExtKey extKey)
            {
                _extKey = extKey;
            }

            public Dependencies.IHDKey Derive(string path)
            {
                return new HDKey(_extKey.Derive(KeyPath.Parse(path)));
            }

            public Dependencies.IHDKey Derive(uint index)
            {
                return new HDKey(_extKey.Derive(index));
                
            }

            public byte[] PublicKey => _extKey.GetPublicKey().ToBytes();
            public byte[] PrivateKey => _extKey.PrivateKey.ToBytes();
            
            public string ExtendedPrivateKey
            { get
                {
                    var bytes = _extKey.ToBytes();
                    var versionBytes = Network.Main.GetVersionBytes(Base58Type.EXT_SECRET_KEY, true);
                    var allBytes = versionBytes.Concat(bytes).ToArray();
                    var encoded = Network.Main.GetBase58CheckEncoder().EncodeData(allBytes);
                    return encoded;
                }
            }
        }

        private class SHA512_256 : Sha512tDigest
        {
            public SHA512_256() : base(256) { }
        }

        private class DefaultBIP39 : IBIP39
        {
            public string GenerateMnemonicPhrase()
            {
                return new Mnemonic(Wordlist.English, WordCount.TwentyFour).ToString();
            }

            public string? MnemonicToSeedHex(string mnemonic, string password)
            {
                try
                {
                    var m = new Mnemonic(mnemonic);
                    return m.DeriveSeed(password).ToHex();
                } catch (Exception e)
                {
                    Log.Trace("MnemonicToSeedHex: " + e.ToString());
                }
                return null;
            }
        }

        private class DefaultHDKey : IHDKeyProvider
        {
            public Dependencies.IHDKey GetFromSeed(string seed)
            {
                return new HDKey(ExtKey.CreateFromSeed(seed.ToHexByteArray()));
            }
        }
    }
}

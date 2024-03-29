﻿using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.Secp256k1;
using Org.BouncyCastle.Crypto.Digests;
using StacksForce.Dependencies;
using StacksForce.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StacksForce.Stacks
{
    static public class StacksDependencies
    {
        static public void SetupDefault()
        {
            if (DependencyProvider.Cryptography == null)
                DependencyProvider.Cryptography = new DefaultCryptography();
            if (DependencyProvider.BtcFeatures == null)
                DependencyProvider.BtcFeatures = new DefaultBtcFeatures();
            if (DependencyProvider.HttpClient == null)
                DependencyProvider.HttpClient = new DefaultHttpClient();
        }

        private class DefaultHttpClient : IHttpClient
        {
            private static readonly HttpHelper.IRetryStrategy DEFAULT_RETRY_STRATEGY = new RetryStrategy();

            public Task<AsyncCallResult<string>> Get(string uri)
            {
                return HttpHelper.SendRequest(uri, null, DEFAULT_RETRY_STRATEGY);
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
                var data = JsonService.Serialize(json, json.GetType());
                var content = HttpHelper.GetJsonContent(data);
                return HttpHelper.SendRequest(uri, content, DEFAULT_RETRY_STRATEGY);
            }

            private class RetryStrategy : HttpHelper.NRetryStrategy
            {
                public RetryStrategy() : base(3, 3500)
                {
                }

                protected override bool AllowRetryForError(Error? error)
                {
                    return error == null || (error is HttpError e &&
                        (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                         e.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                         e.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                        );
                }
            }
        }

        private class DefaultCryptography : ICryptography
        {
            public byte[] RipeMD160(byte[] data) => Hashes.RIPEMD160(data);

            public byte[] Secp256k1GetPublicKey(byte[] privateKey, bool compressed)
            {
                if (!ECPrivKey.TryCreate(privateKey, null, out var pKey))
                    throw new ArgumentException("private key");

                var pubKey = pKey.CreatePubKey();

                var bytes = new byte[compressed ? 33 : 65];
                pubKey.WriteToSpan(compressed, bytes, out _);
                return bytes;
            }

            public byte[] Secp256k1Sign(byte[] data, byte[] privateKey, out int recoveryId)
            {
                if (!ECPrivKey.TryCreate(privateKey, null, out var pKey))
                    throw new ArgumentException("private key");

                if (!pKey.TrySignRecoverable(data, out var sig))
                    throw new ArgumentException("data");

                var bytes = new byte[64];
                sig.WriteToSpanCompact(bytes, out recoveryId);
                return bytes;
            }

            public bool Secp256k1Verify(byte[] message, byte[] signature, byte[] pubkey)
            {
                if (!SecpECDSASignature.TryCreateFromCompact(signature, out var s))
                    return false;

                ECPubKey? ecPubKey = null;
                if (pubkey.Length == 64)
                {
                    GE point = new GE(
                        new FE(new ReadOnlySpan<byte>(pubkey, 0, 32)),
                        new FE(new ReadOnlySpan<byte>(pubkey, 32, 32)));
                    ecPubKey = new ECPubKey(point, null);
                } else { 
                if (!ECPubKey.TryCreate(pubkey, null, out _, out ecPubKey))
                    return false;
                }
                return ecPubKey.SigVerify(s, message);
            }

            public byte[] Sha256(byte[] data) => Hashes.SHA256(data);

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

        private class DefaultBtcFeatures : IBtcFeatures
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

            public string? GetSegwitAddress(string publicKey)
            {
                var pubKey = new PubKey(publicKey);
                return pubKey.GetAddress(ScriptPubKeyType.Segwit, Network.Main).ToString();
            }

            public string? PrivateKeyToWif(string privateKey)
            {
                var pKey = new Key(privateKey.ToHexByteArray());
                return pKey.GetWif(Network.Main).ToWif();
            }

            public string? WifToPrivateKey(string wif)
            {
                var pKey = Key.Parse(wif, Network.Main);
                return pKey.ToHex();
            }

            public Dependencies.IHDKey GetFromSeed(string seed)
            {
                return new HDKey(ExtKey.CreateFromSeed(seed.ToHexByteArray()));
            }

        }
    }
}

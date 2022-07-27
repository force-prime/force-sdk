using StacksForce.Dependencies;
using StacksForce.Utils;
using System.Linq;

namespace StacksForce.Stacks
{
    static public class SigningUtils
    {
        private const int PRIVATE_KEY_COMPRESSED_LENGTH = 33;

        public static readonly byte[] EMPTY_SIG_65 = Enumerable.Range(0, 65).Select(x => (byte)0).ToArray();

        static public PubKeyEncoding GetPubKeyEncoding(string pubKey)
        {
            return !pubKey.StartsWith("04") ? PubKeyEncoding.Compressed : PubKeyEncoding.Uncompressed;
        }

        static public string GetPublicKeyFromPrivateKey(string privateKey, bool compressed = true)
        {
            var buf = privateKey.ToHexByteArray();
            var pubKey = SigningUtils.Secp256k1GetPublicKey(buf.Slice(0, 32), compressed);
            return pubKey.ToHex();
        }

        static public byte[] CompressPrivateKey(byte[] key)
        {
            if (key.Length == PRIVATE_KEY_COMPRESSED_LENGTH)
                return key;
            return key.Concat(new byte[1] { 1 }).ToArray();
        }

        static public string TxIdFromData(byte[] data)
        {
            return DependencyProvider.Cryptography.Sha512_256(data).ToHex();
        }

        static public byte[] Secp256k1Sign(byte[] data, byte[] privateKey)
        {
            var sig = DependencyProvider.Cryptography.Secp256k1Sign(data, privateKey.Slice(0, 32), out var recoveryId);
            return new byte[1] { (byte)recoveryId }.Concat(sig).ToArray();
        }

        static public byte[] Secp256k1GetPublicKey(byte[] privateKey, bool compressed)
        {
            return DependencyProvider.Cryptography.Secp256k1GetPublicKey(privateKey, compressed);
        }

        static public byte[] Sha256(byte[] bytes)
        {
            return DependencyProvider.Cryptography.Sha256(bytes);
        }

        static public byte[] Sha256RipeMD160(byte[] bytes)
        {
            var hash = Sha256(bytes);
            return RipeMD160(hash);
        }

        public static byte[] RipeMD160(byte[] bytes)
        {
            return DependencyProvider.Cryptography.RipeMD160(bytes);
        }

        static public byte[] HashP2PKH(string publicKey)
        {
            var bytes = ByteUtils.ToHexByteArray(publicKey);
            var hash = Sha256RipeMD160(bytes);
            return hash;
        }

    }
}

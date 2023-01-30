using StacksForce.Dependencies;
using StacksForce.Utils;
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Text;

namespace StacksForce.Stacks
{
    static public class SigningUtils
    {
        private const string SIGNATURE_CHAIN_PREFIX = "\x17Stacks Signed Message:\n";

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

        static public string SignRsv(this StacksAccount account, string message)
        {
            var bytes2Hash = Encoding.UTF8.GetBytes(SIGNATURE_CHAIN_PREFIX).Concat(
                SerializeBitcoinVarInt((ulong)message.Length)).Concat(
                    Encoding.UTF8.GetBytes(message)).ToArray();

            var sha = Sha256(bytes2Hash);

            var signature = account.Sign(sha);
            var v = signature.Take(1).ToArray();
            var r = signature.Skip(1).Take(32).ToArray();
            var s = signature.Skip(33).Take(32).ToArray();

            return r.ToHex() + s.ToHex() + v.ToHex();
        }

        static private byte[] SerializeBitcoinVarInt(ulong value)
        {
            byte[] bytes;

            if (value < 0xFD)
            {
                return new byte[1] {(byte) value};
            }
            else if (value <= 0xffff)
            {
                bytes = new byte[3];
                bytes[0] = 0xfd;
                BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(bytes, 1, 2), (ushort) value);
            }
            else if (value <= 0xffffffff)
            {
                bytes = new byte[5];
                bytes[0] = 0xfe;
                BinaryPrimitives.WriteUInt32LittleEndian(new Span<byte>(bytes, 1, 4), (uint)value);
            }
            else
            {
                bytes = new byte[9];
                bytes[0] = 0xff;
                BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(bytes, 1, 8), value);
            }
            return bytes;
        }

        static public uint GetStringHashCode(string str)
        {
            uint hash = 0;
            if (string.IsNullOrEmpty(str))
                return hash;

            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                hash = (hash << 5) - hash + c;
            }
            return hash & 0x7fffffff;
        }

    }
}

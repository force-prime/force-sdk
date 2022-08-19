using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StacksForce.Utils
{
    static public class ByteUtils
    {
        static public string ToHex(this byte[] bytes) => ToHex(new ReadOnlySpan<byte>(bytes));

        static public string ToHex(this ReadOnlySpan<byte> bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        static public string UintToHex(ulong value)
        {
            return string.Format("{0:x8}", value);
        }

        static public string UlongToHex(ulong value)
        {
            return string.Format("{0:x16}", value);
        }

        static public string ByteToHex(byte value)
        {
            return string.Format("{0:x2}", value);
        }

        static public byte[] UInt32ToByteArrayBigEndian(UInt32 value)
        {
            byte[] bytes = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            return bytes;
        }

        static public byte[] UInt64ToByteArrayBigEndian(ulong value)
        {
            byte[] bytes = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
            return bytes;
        }

        static public byte[] Int128ToByteArrayBigEndian(BigInteger intValue)
        {
            if (intValue > 0)
                return intValue.ToByteArray(true, true).PadLeft(16);

            var max = new BigInteger(Enumerable.Range(0, 16).Select(x => (byte) 0xff).ToArray(), true, true);
            intValue += 1 + max;
            return intValue.ToByteArray(true, true);
        }

        static public byte[] UInt64ToByteArrayBigEndian(ulong value, int byteCount)
        {
            return Enumerable.Range(8 - byteCount, byteCount).Select(x => (byte)((value >> ((7 - x) * 8)) % 256)).ToArray();
        }

        static public byte[] ToHexByteArray(this string hex)
        {
            hex = hex.ToLowerInvariant();
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            return Enumerable.Range(0, hex.Length / 2).Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16)).ToArray();
        }

        static public byte[] Slice(this byte[] bytes, int start) {
            if (start >= 0)
                return Slice(bytes, start, bytes.Length - start);
            else
                return Slice(bytes, bytes.Length + start, Math.Abs(start));
        }

        static public byte[] Slice(this byte[] bytes, int start, int length)
        {
            byte[] result = new byte[length];
            Buffer.BlockCopy(bytes, start, result, 0, length);
            return result;
        }

        static public byte[] TrimEnd(this byte[] bytes, byte trimByte = 0)
        {
            var len = bytes.Length - 1;
            while (len >= 0 && bytes[len] == trimByte)
                len--;
            return bytes.Slice(0, len + 1);
        }

        static public byte[] PadLeft(this byte[] bytes, int totalLen, byte padByte = 0)
        {
            if (bytes.Length >= totalLen)
                return bytes;

            return Enumerable.Range(0, totalLen - bytes.Length).Select(x => padByte).Concat(bytes).ToArray();
        }

        static public byte[] PadRight(this byte[] bytes, int totalLen, byte padByte = 0)
        {
            if (bytes.Length >= totalLen)
                return bytes;

            return bytes.Concat(Enumerable.Range(0, totalLen - bytes.Length).Select(x => padByte)).ToArray();
        }

        static public string AsciiFromHex(string hexString)
        {
            return System.Text.Encoding.ASCII.GetString(hexString.ToHexByteArray());
        }

        static public string ToHexString(this IBinarySerializable serializable)
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                serializable.SerializeTo(writer);
                var data = stream.ToArray();
                return data.ToHex();
            }
        }
    }

    public interface IBinarySerializable
    {
        void SerializeTo(BinaryWriter writer);
    }
}

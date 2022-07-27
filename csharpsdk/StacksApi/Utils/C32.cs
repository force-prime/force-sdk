using System;
using System.Security.Cryptography;
using System.Text;

namespace StacksForce.Utils
{
    static public class C32
    {
        private const string Symbols = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private static readonly sbyte[] ValueBySymbol =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
             0,  1,  2,  3,  4,  5,  6,  7,  8,  9, -1, -1, -1, -1, -1, -1, /*
                 A   B   C   D   E   F   G   H   I   J   K   L   M   N   O   */
            -1, 10, 11, 12, 13, 14, 15, 16, 17,  1, 18, 19,  1, 20, 21,  0, /*
             P   Q   R   S   T   U   V   W   X   Y   Z                       */
            22, 23, 24, 25, 26, -1, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1, /*
                 a   b   c   d   e   f   g   h   i   j   k   l   m   n   o   */
            -1, 10, 11, 12, 13, 14, 15, 16, 17,  1, 18, 19,  1, 20, 21,  0, /*
             p   q   r   s   t   u   v   w   x   y   z                       */
            22, 23, 24, 25, 26, -1, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        public static string EncodeBigEndian(byte[] buffer)
        {
            var sb = new StringBuilder();
            EncodeBigEndian(buffer, sb);
            return sb.ToString();
        }

        public static void EncodeBigEndian(byte[] buffer, StringBuilder output)
        {
            ushort bb = 0;
            var bits = 0;
            for (int length = buffer.Length; length > 0; length--)
            {
                bb |= (ushort)(buffer[length - 1] << bits);
                bits += 8;
                for (; bits >= 5; bits -= 5)
                {
                    output.Insert(0, Symbols[bb & 31]);
                    bb >>= 5;

                }
            }
            if (bb > 0)
                output.Insert(0, Symbols[bb & 31]);
        }

        public static string Normalize(string input)
        {
            return input.ToUpperInvariant()
                .Replace("O", "0")
                .Replace("L", "1")
                .Replace("I", "1");
        }

        public static string AddressEncode(int version, string data)
        {
            var vh = ByteUtils.ByteToHex((byte) version);
            var checksum = Checksum(vh + data);
            return "S" + Symbols[version] + EncodeBigEndian((data + checksum).ToHexByteArray());
        }

        public static (byte version, string data)? AddressDecode(string input)
        {
            input = Normalize(input);

            if (!input.StartsWith("S"))
                return null;

            var versionChar = input[1];
            byte version = (byte) Symbols.IndexOf(versionChar);

            input = input.Substring(2); 

            var bytes = new byte[input.Length * 5 / 8 + (input.Length % 8 == 0 ? 0 : 1)];
            DecodeBigEndian(input, bytes);

            var addressBytes = bytes.Slice(0, bytes.Length - 4);

            int hexLeadingZeros = 0;
            for (; hexLeadingZeros < addressBytes.Length; hexLeadingZeros++)
                if (bytes[hexLeadingZeros] != 0)
                    break;
            addressBytes = addressBytes.Slice(hexLeadingZeros);

            var address = addressBytes.ToHex();
            var addressChecksum = Checksum(ByteUtils.ByteToHex(version) + address);
            var checksum = bytes.Slice(-4).ToHex();

            if (checksum != addressChecksum)
                return null;

            return (version, address);
        }

        private static string Checksum(string data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(sha.ComputeHash(data.ToHexByteArray())).ToHex().Substring(0, 8);
            }
        }

        private static void DecodeBigEndian(string input, byte[] buffer)
        {
            var index = buffer.Length - 1;
            var bits = 0;
            uint currentValue = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                var ch = input[i];
                var b = ValueBySymbol[ch];

                if (b < 0)
                    throw new FormatException($"'{ch}' is an invalid symbol.");

                currentValue += (uint)b << bits;
                bits += 5;

                for (; bits >= 8; bits -= 8)
                {
                    buffer[index] = (byte)(currentValue & 255);
                    currentValue >>= 8;
                    index--; 
                }
            }
            if (bits > 0)
                buffer[index] = (byte)currentValue;
        }
    }
}

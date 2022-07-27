using StacksForce.Utils;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StacksForce.Stacks
{
    static public class SerializationUtils
    {
        private const int SERIALIZED_ADDRESS_LENGTH = 21;

        static public byte[] SerializeAddress(string addressHash, byte version)
        {
            return new byte[] { version }.Concat(addressHash.ToHexByteArray()).ToArray();
        }

        static public byte[] SerializeAddress(string address)
        {
            var addr = Address.FromC32(address)!.Value;
            return SerializeAddress(addr.hash160, (byte) addr.version);
        }

        static public byte[] SerializeLPString(string str, int lengthSizeBytes = 1)
        {
            var prefix = ByteUtils.UInt64ToByteArray((ulong)str.Length, lengthSizeBytes);
            var strBytes = Encoding.ASCII.GetBytes(str);
            return prefix.Concat(strBytes).ToArray();
        }

        static public void SerializeLPList(IBinarySerializable[] list, BinaryWriter writer)
        {
            writer.Write(ByteUtils.UInt32ToByteArray((uint) list.Length));
            foreach (var e in list)
                e.SerializeTo(writer);
        }

        static public byte[] SerializeBigUInteger(BigInteger value)
        {
            return value.ToByteArray(true, true).PadLeft(16);
        }

        static public byte[] SerializeBigInteger(BigInteger value)
        {
            return value.ToByteArray(false, true).PadLeft(16);
        }

        static public string? DeserializeAddress(byte[] bytes)
        {
            if (bytes.Length != SERIALIZED_ADDRESS_LENGTH)
                return null;

            var addressVer = ConstantUtils.AddressVersionFromByte(bytes[0]);
            if (addressVer == null)
                return null;

            return Address.AddressFromVersionHash(addressVer.Value, bytes.ToHex());
        }
    }
}

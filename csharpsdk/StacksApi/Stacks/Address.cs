using StacksForce.Utils;

namespace StacksForce.Stacks
{
    static public class Address
    {
        public static (string hash160, AddressVersion version)? FromC32(string c32AddressString)
        {
            if (string.IsNullOrEmpty(c32AddressString))
                return null;

            var address = C32.AddressDecode(c32AddressString);
            if (!address.HasValue)
                return null;

            var addressData = address!.Value;
            AddressVersion? ver = ConstantUtils.AddressVersionFromByte(addressData.version);
            if (!ver.HasValue)
                return null;

            return (addressData.data, ver.Value);
        }

        static public string AddressFromPublicKey(AddressVersion version, AddressHashMode hashMode, string pubKey)
        {
            var c = AddressFromPublicKey(hashMode, pubKey);
            return AddressFromVersionHash(version, c.ToHex());
        }

        static public string AddressFromVersionHash(AddressVersion version, string hash)
        {
            return C32.AddressEncode((byte)version, hash);
        }

        static public byte[] AddressFromPublicKey(AddressHashMode hashMode, string pubKey)
        {
            switch (hashMode)
            {
                case AddressHashMode.SerializeP2PKH:
                    return SigningUtils.HashP2PKH(pubKey);
            }
            return null;
        }

        static public (string address, string contract, string token) ParseFromFullTokenId(string fullTokenId)
        {
            var full = fullTokenId.Split(new string[] { "::" }, System.StringSplitOptions.None);
            var addressAndContract = full[0].Split('.');
            return (addressAndContract[0], addressAndContract[1], full[1]);
        }
    }
}

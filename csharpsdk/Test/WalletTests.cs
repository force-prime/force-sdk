using StacksForce.Dependencies;
using StacksForce.Stacks;
using StacksForce.Utils;

namespace StacksForceTest
{
    public class WalletTests
    {
        static WalletTests()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public static void TestCompressedPubKeyFromPrivKey()
        {
            const string expected = "02805e7ccda11e83be85543ff90166db4ac24c84aaeafc85c5c4980a3e95a394a6";

            string stxPrivateKey = "8721c6a5237f5e8d361161a7855aa56885a3e19e2ea6ee268fb14eabc5e2ed90";
            var pubKey = SigningUtils.GetPublicKeyFromPrivateKey(stxPrivateKey, true);

            Assert.Equal(expected, pubKey);
        }

        [Fact]
        public static void TestUncompressedPubKeyFromPrivKey()
        {
            const string expected = "04805e7ccda11e83be85543ff90166db4ac24c84aaeafc85c5c4980a3e95a394a6b6b92ecc576252c36006c72dfbdf9b551c618bb3fd9d79f908c428736755ce2a";

            string stxPrivateKey = "8721c6a5237f5e8d361161a7855aa56885a3e19e2ea6ee268fb14eabc5e2ed90";
            var pubKey = SigningUtils.GetPublicKeyFromPrivateKey(stxPrivateKey, false);

            Assert.Equal(expected, pubKey);
        }

        [Fact]
        public static void TestPubKeyToAddress()
        {
            const string expected = "SP1GZ804XH4240T4JT2GQ34GG0DMT6B3BQ5NV18PD";

            string stxPrivateKey = "8721c6a5237f5e8d361161a7855aa56885a3e19e2ea6ee268fb14eabc5e2ed90";
            var pubKey = SigningUtils.GetPublicKeyFromPrivateKey(stxPrivateKey, false);
            var address = Address.AddressFromPublicKey(AddressVersion.MainnetSingleSig, AddressHashMode.SerializeP2PKH, pubKey);

            Assert.Equal(expected, address);
        }

        [Fact]
        public static void TestExtKey()
        {
            const string expected = "xprv9s21ZrQH143K4J83CexJ2sTfgzTZvshpYwUPzx79dwP3GsfkmAjS4GZcx2p4CWt5TZ1QPJ8JsiL1x7L3ChXXZV1np16yFt2uJduyBpN5dRC";

            var phrase = "gesture clip cube census regular engage kit ask cereal wrong awkward often glance behave issue lucky erosion harbor guide clever reduce theme surge spell";
            var seed = DependencyProvider.BtcFeatures.MnemonicToSeedHex(phrase, "");
            var key = DependencyProvider.BtcFeatures.GetFromSeed(seed);
            Assert.Equal(key.ExtendedPrivateKey, expected);
        }

        [Fact]
        public static void TestWallet()
        {
            const string expectedTestNetAddr = "ST384CVPNDTYA0E92TKJZQTYXQHNZSWGCAH0ER64E";
            const string expectedMainNetAddr = "SP384CVPNDTYA0E92TKJZQTYXQHNZSWGCAG7SAPVB";
            const string expectedSalt = "c15619adafe7e75a195a1a2b5788ca42e585a3fd181ae2ff009c6089de54ed9e";
            const string expectedAppKey = "6f8b6a170f8b2ee57df5ead49b0f4c8acde05f9e1c4c6ef8223d6a42fabfa314";

            var w = new StacksWallet("sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media", "");
            Assert.Equal(expectedSalt, w.Salt);

            var account = w.GetAccount(0);

            Assert.Equal(expectedAppKey, account.GetAppPrivateKey("https://banter.pub"));
            Assert.Equal(expectedTestNetAddr, account.GetAddress(AddressVersion.TestnetSingleSig));
            Assert.Equal(expectedMainNetAddr, account.GetAddress(AddressVersion.MainnetSingleSig));
        }

        [Fact]
        static public void TestP2PKH()
        {
            const string expected = "61f4009d8904406892d0a17192100369a32c6bb9";

            string stxPrivateKey = "8721c6a5237f5e8d361161a7855aa56885a3e19e2ea6ee268fb14eabc5e2ed90";
            var pubKey = SigningUtils.GetPublicKeyFromPrivateKey(stxPrivateKey, false);
            var address = Address.AddressFromPublicKey(AddressHashMode.SerializeP2PKH, pubKey);

            Assert.Equal(expected, address.ToHex());
        }


        [Fact]
        static public void TestC32AddressDecode()
        {
            var expected = (22, "df0ba3e79792be7be5e50a370289accfc8c9e032");
            var expected2 = (26, "3742176830998a306cb3820fae78ad19a178f3e3");

            var decoded = C32.AddressDecode("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159");
            Assert.Equal(expected, decoded.Value);

            var decoded2 = C32.AddressDecode("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ");
            Assert.Equal(expected2, decoded2.Value);
        }

        [Fact]
        static public void TestC32Encode()
        {
            const string expected = "VW5T7SWQJAZ7QSF518VG52DCSZ4CKR1J";
            var encoded = C32.EncodeBigEndian("df0ba3e79792be7be5e50a370289accfc8c9e032".ToHexByteArray());
            Assert.Equal(expected, encoded);
        }

        [Fact]
        static public void TestC32AddressEncode()
        {
            const string expected = "S13TMMP2HX4VJQ1W11A761KEJWZ7XG2VE6478XAW9";
            var adr = C32.AddressEncode(1, "f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
            Assert.Equal(expected, adr);
        }

        [Fact]
        static public void TestSegwitAddress()
        {
            var mnemonic = "token spatial butter drill city debate pipe shoot target pencil tonight gallery dog globe copy hybrid convince spell load maximum impose crazy engage way";
            var w = new StacksWallet(mnemonic);
            var account = w.GetBTCAccount(1);
            Assert.Equal("bc1q5aptjy5l9q4qcykvccpwlqcvzydg744qkv94d3", account.GetBtcSegwitAddress());
        }
    }
}
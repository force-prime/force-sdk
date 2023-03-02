using StacksForce.Stacks;
using StacksForce.Utils;

namespace StacksForceTest
{
    public class CryptographyTests
    {

        private const string PUBLIC_KEY = "0490255f88fa311f5dee9425ce33d7d516c24157e2aae8e25a6c631dd6f7322aefcb6ef061bb8ab4c15f4a92b719264fc13c76cdba7f5aa24a4067a5e8405b7b56";
        private const string PRIVATE_KEY = "bcf62fdd286f9b30b2c289cce3189dbf3b502dcd955b2dc4f67d18d77f3e73c7";

        static CryptographyTests()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public static void TestSignature1()
        {
            var data = Enumerable.Range(0, 32).Select(x => (byte) x).ToArray();
            var signature = SigningUtils.Secp256k1Sign(data, PRIVATE_KEY.ToHexByteArray());
            var publicKey = SigningUtils.GetPublicKeyFromPrivateKey(PRIVATE_KEY, false);

            var verified = SigningUtils.Secp256k1Verify(data, signature, publicKey.ToHexByteArray());

            Assert.Equal(PUBLIC_KEY, publicKey);
            Assert.True(verified);
        }

        [Fact]
        public static void TestSignature2()
        {
            var data = Enumerable.Range(0, 32).Select(x => (byte)x).ToArray();
            var account = new StacksAccountBase(PRIVATE_KEY);
            var signature = account.Sign(data);
            var verified = account.Verify(data, signature);
            Assert.True(verified);
        }

        [Fact]
        public static void TestSignature3()
        {
            var account = new StacksAccountBase(PRIVATE_KEY);
            var message = "hello world 123";

            var signatureRsv = account.StacksSignRsv(message);
            var verified = account.VerifyStacksSignRsv(message, signatureRsv);
            Assert.True(verified); 
        }
    }
}

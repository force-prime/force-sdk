using StacksForce.Stacks;

namespace ShortDemos
{
    public static class SignMessage
    {
        private const string WALLET_MNEMONIC = "sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media";

        public static void Test()
        {
            StacksWallet w = new StacksWallet(WALLET_MNEMONIC);
            var account = w.GetAccount(0);

            if (account == null)
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            Console.WriteLine("Account testnet address: " + account.GetAddress(AddressVersion.TestnetSingleSig));
            Console.WriteLine("Account public key: " + account.PublicKey);

            var messageToSign = "hello world";

            var signature = account.StacksSignRsv(messageToSign);

            Console.WriteLine("Signature: ");
            Console.WriteLine(signature);
        }
    }
}

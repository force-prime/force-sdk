using ChainAbstractions.Stacks;

namespace ShortDemos
{
    static public class CallReadOnly
    {
        // for demo purpose only - never ever put your mnemonic anywhere, it should be always kept private
        private const string WALLET_MNEMONIC = "sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media";

        static public async Task Demo()
        {
            var wallet = StacksAbstractions.TestNet.GetWalletForMnemonic(WALLET_MNEMONIC);
            Console.WriteLine("Performing read only call...");

            var methodCall = await StacksAbstractions.TestNet.CallReadOnly("ST2SDZYR4VQF138X2A76KEFFRC6A834MDXXWFRZW1",
                "ST2SDZYR4VQF138X2A76KEFFRC6A834MDXXWFRZW1.basic-token", "get-name", null);

            // expected success with value 'GAME-CURRENCY'
            if (methodCall.Error == null)
            {
                Console.WriteLine("Success, result = " + methodCall.Data.ToString());
            } else
            {
                Console.WriteLine("Call failed: " + methodCall.Error.ToString());
            }
        }
    }
}

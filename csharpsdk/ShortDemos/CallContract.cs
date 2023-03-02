using ChainAbstractions.Stacks;
using ChainAbstractions;

namespace ShortDemos
{
    static public class CallContract
    {
        // for demo purpose only - never ever put your mnemonic anywhere, it should be always kept private
        private const string WALLET_MNEMONIC = "sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media";

        static public async Task Demo()
        {
            var wallet = StacksAbstractions.TestNet.GetWalletForMnemonic(WALLET_MNEMONIC);
            Console.WriteLine("Requesting call...");

            var methodCall = await wallet.GetContractCallTransaction("ST2SDZYR4VQF138X2A76KEFFRC6A834MDXXWFRZW1.basic-nfts", "mint",
                new List<IVariable> { "ST2SDZYR4VQF138X2A76KEFFRC6A834MDXXWFRZW1".ToAddressVariable(), ((ulong)33).ToVariable() });

            if (methodCall.Error == null)
            {
                Console.WriteLine($"Sending for {methodCall.Cost.BalanceFormatted()}...");
                await methodCall.Send();
            }

            var currentState = methodCall.State;
            while (true)
            {
                if (currentState != methodCall.State)
                {
                    currentState = methodCall.State;
                    Console.WriteLine($"State changed to {currentState}");
                }

                if (methodCall.Error != null)
                {
                    Console.WriteLine("Call failed: " + methodCall.Error);
                    return;
                }
                else if (methodCall.State == ChainAbstractions.TransactionState.Approved)
                    break;

                await Task.Delay(5000);
            }

            // method is expected to fail
            Console.WriteLine("Request completed: " + methodCall.State);
        }
    }
}

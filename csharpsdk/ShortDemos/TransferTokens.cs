using ChainAbstractions.Stacks;

namespace ShortDemos
{
    static public class TransferTokens
    {
        // for demo purpose only - never ever put your mnemonic anywhere, it should be always kept private
        private const string WALLET_MNEMONIC = "sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media";

        static public async Task Demo()
        {
            var wallet = StacksAbstractions.TestNet.GetWalletForMnemonic(WALLET_MNEMONIC);
            Console.WriteLine("Requesting transfer");
            var transfer = await wallet.GetTransferTransaction(StacksAbstractions.Stx.From(1.5), "ST1QK1AZ24R132C0D84EEQ8Y2JDHARDR58SMAYMMW", "yeah!");

            if (transfer.Error == null)
            {
                Console.WriteLine("Sending...");
                await transfer.Send();
            }

            var currentState = transfer.State;
            while (true)
            {
                if (currentState != transfer.State)
                {
                    currentState = transfer.State;
                    Console.WriteLine($"State changed to {currentState}");
                }

                if (transfer.Error != null)
                {
                    Console.WriteLine("Transfer failed: " + transfer.Error);
                    return;
                }
                else if (transfer.State == ChainAbstractions.TransactionState.Approved)
                    break;

                await Task.Delay(5000);
            }

            Console.WriteLine("Request completed");
        }
    }
}

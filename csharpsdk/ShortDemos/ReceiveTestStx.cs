using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;

namespace ShortDemos
{
    static public class ReceiveTestStx
    {
        // for demo purpose only - never ever put your mnemonic anywhere, it should be always kept private
        private const string WALLET_MNEMONIC = "sound idle panel often situate develop unit text design antenna vendor screen opinion balcony share trigger accuse scatter visa uniform brass update opinion media";

        static public async Task Demo()
        {
            var wallet = StacksAbstractions.TestNet.GetWalletForMnemonic(WALLET_MNEMONIC);
            var address = wallet.GetAddress();

            await GetCurrentBalance(wallet);

            Console.WriteLine("Requesting test STX");

            // this will create a blockchain transaction that sends STX to given address
            var result = await Blockchains.Testnet.GetSTXTestnetTokens(address);
            if (result.IsError)
            {
                Console.WriteLine("Something went wrong");
                return;
            }
            // let's wait a bit
            await Task.Delay(5000);

            var info = await TransactionInfo.ForTxId(Blockchains.Testnet, result.Data);
            if (info == null)
            {
                Console.WriteLine("Can't get transaction info");
                return;
            }

            Console.WriteLine("Transaction: " + info.ToString());
            Console.WriteLine($"Check transaction state in stacks explorer: {info.StacksExplorerLink}");

            Console.WriteLine("Wait for transaction to complete...");

            while (true)
            {
                if (info.Status != TransactionStatus.Pending)
                {
                    if (info.Status != TransactionStatus.Success || info.IsAnchored)
                        break;
                }

                await Task.Delay(5000);
                await info.Refresh();
            }

            if (info.Status != TransactionStatus.Success)
            {
                Console.WriteLine("Transaction failed with status: " + info.Status);
                return;
            }

            await GetCurrentBalance(wallet);
        }

        static private async Task GetCurrentBalance(IBasicWallet wallet)
        {
            Console.WriteLine("Fetching current balance");

            var currentStxBalance = await wallet.GetToken(null);
            if (currentStxBalance == null)
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            Console.WriteLine("Current balance: " + currentStxBalance.BalanceFormatted());
        }
    }
}

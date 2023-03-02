using ChainAbstractions;
using ChainAbstractions.Stacks;

namespace ShortDemos
{
    static public class GenerateWallet
    {
        static public async Task Demo()
        {
            // for more information about wallets and accounts see
            // https://docs.stacks.co/docs/understand-stacks/accounts
            // https://www.hiro.so/wallet-faq/what-are-secret-keys-and-how-do-they-work

            Console.WriteLine("Start wallet generation");
            var wallet = StacksAbstractions.TestNet.CreateNewWallet();
            Console.WriteLine("Wallet generated");
            Console.WriteLine("Mnemonic: " + wallet.GetMnemonic());
            Console.WriteLine("Address: " + wallet.GetAddress());

            Console.WriteLine("Wallet contents: ");
            var tokens = await wallet.GetAllTokens();
            if (tokens.IsError)
            {
                Console.WriteLine("Error occured: " + tokens.Error);
                return;
            }

            foreach (var t in tokens.Data)
                Console.WriteLine(t.BalanceFormatted());

            Console.WriteLine("Wallet should be empty :)");
        }
    }
}

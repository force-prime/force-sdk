using ChainAbstractions;
using ChainAbstractions.Stacks;

namespace ShopSample
{
    internal class Utils
    {
        static public async Task SendAndWait(string name, ITransaction transaction)
        {
            Console.WriteLine("Sending transaction: " + name);

            await transaction.Send();

            Console.WriteLine("Transactions broadcasted: " + name);
            Console.WriteLine("Waiting for transaction to be approved...");
            Console.WriteLine("Check status in stacks explorer: " + transaction.GetStacksExplorerLink());

            while (transaction.Error == null && (transaction.State == TransactionState.Pending || transaction.State == TransactionState.PreApproved))
                await Task.Delay(10000);

            if (transaction.Error != null)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine($"Transaction '{name}' failed: {transaction.Error}");
                Environment.Exit(1);
            }
            else
                Console.WriteLine($"Transaction '{name}' success");
        }
    }
}

using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;

namespace ShortDemos
{
    public class GetRecentTransactions
    {
        static public async Task Demo()
        {
            TransactionInfoStream stream = new TransactionInfoStream(Blockchains.Mainnet, false);

            var transactions = await stream.ReadMoreAsync(20);
            foreach (var t in transactions)
            {
                Console.WriteLine($"{t.TxId}: {t.Type} {t.Status}");
                var eventStream = t.Events.GetStream();
                var events = await eventStream.ReadMoreAsync(10);
                foreach (var e in events)
                {
                    Console.WriteLine($"    {e.ToString()}");
                }
            }
        }

        static public async Task CalcStats()
        {
            Console.WriteLine("Reading...");

            TimeSpan duration = TimeSpan.FromHours(2);
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            ulong totalTransferred = 0;
            ulong totalNftMinted = 0;
            ulong totalNftTransferred = 0;
            ulong transactionsCount = 0;

            TransactionInfoStream stream = new TransactionInfoStream(Blockchains.Mainnet, false);
            while (true)
            {
                var transactions = await stream.ReadMoreAsync(30);

                if (transactions == null || transactions.Count == 0)
                    break;

                transactionsCount += (ulong)transactions.Count;

                if (end - start > duration)
                    break;

                foreach (var t in transactions)
                {
                    if (t.BurnBlockTime < start)
                        start = t.BurnBlockTime;
                    if (t.BurnBlockTime > end)
                        end = t.BurnBlockTime;

                    var eventStream = t.Events.GetStream();
                    while (true)
                    {
                        var events = await eventStream.ReadMoreAsync(50);
                        if (events == null || events.Count == 0)
                            break;
                        foreach (var e in events)
                        {
                            if (e is StxEvent stxEvent && stxEvent.Type == TransactionEvent.TokenEventType.Transfer)
                                totalTransferred += stxEvent.Amount;

                            if (e is NFTEvent nftEvent)
                            {
                                if (nftEvent.Type == TransactionEvent.TokenEventType.Mint)
                                    totalNftMinted += 1;
                                else if (nftEvent.Type == TransactionEvent.TokenEventType.Transfer)
                                    totalNftTransferred += 1;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Statistics for period {start} -> {end}");
            Console.WriteLine($"Total transactions: {transactionsCount}");
            Console.WriteLine($"Total stx transferred: {StacksAbstractions.Stx.FormatCount(totalTransferred)}");
            Console.WriteLine($"Total nft minted: {totalNftMinted}");
            Console.WriteLine($"Total nft transferred: {totalNftTransferred}");
        }
    }
}

using ChainAbstractions;
using ChainAbstractions.Stacks;

namespace ShortDemos
{
    public static class ReadWallet
    {
        static public IWalletInfo GetWalletInfoForAddress(string address)
        {
            var chain = StacksAbstractions.FromAddress(address);
            var walletInfo = chain.GetWalletInfoForAddress(address);
            return walletInfo;
        }

        static public async Task PrintAllFungibleTokens(IWalletInfo walletInfo)
        {
            Console.WriteLine("Reading all fungible tokens...");
            var tokens = await walletInfo.GetAllTokens();

            if (tokens.IsError)
            {
                Console.WriteLine("Error occured: " + tokens.Error);
                return;
            }

            Console.WriteLine($"Tokens for {walletInfo.GetAddress()}:");

            foreach (var t in tokens.Data)
            {
                Console.WriteLine($"{t.BalanceFormatted()} ({t.Data.Description})");
            }
        }

        static public async Task PrintNFTs(IWalletInfo walletInfo, string? nftType = null, bool readMetaData = true)
        {
            var nftStream = walletInfo.GetNFTs(nftType, readMetaData);

            while (true) {
                Console.WriteLine("Reading nfts...");
                var nfts = await nftStream.ReadMoreAsync(5);
                if (nfts == null)
                {
                    Console.WriteLine("Error reading nfts");
                    break;
                }

                if (nfts.Count == 0)
                {
                    Console.WriteLine("That's all.");
                    break;
                }

                foreach (var nft in nfts)
                {
                    Console.WriteLine($"{nft.Name}({nft.Description})");
                }

                Console.WriteLine("Press Enter for more, enter any character to complete");

                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                    break;
            }
        }

        static public async Task Demo()
        {
            IWalletInfo? wallet = null;
            try
            {
                wallet = GetWalletInfoForAddress("SPNWZ5V2TPWGQGVDR6T7B6RQ4XMGZ4PXTEE0VQ0S.marketplace-v4");
            } catch (ArgumentException e)
            {
                Console.WriteLine("Incorrect wallet address");
                return;
            }

            await PrintAllFungibleTokens(wallet);

            Console.WriteLine("Read all nfts");
            await PrintNFTs(wallet, null, false);

            var nftType = "SP213KNHB5QD308TEESY1ZMX1BP8EZDPG4JWD0MEA.web4::digital-land";
            Console.WriteLine($"Read nfts of type {nftType}");
            await PrintNFTs(wallet, nftType, true);
        }
    }
}

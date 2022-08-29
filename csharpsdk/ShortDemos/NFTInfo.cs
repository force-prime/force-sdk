using ChainAbstractions.Stacks.ContractWrappers;

namespace ShortDemos
{
    static public class NFTInfo
    {
        static public async Task GetNFTData(string nftType, ulong nftId)
        {
            var sip09 = new SIP09UnsignedInteger(nftType);
            var result = await sip09.GetTokenOwner(nftId);

            Console.WriteLine();
            Console.WriteLine($"NFT {nftId}");

            if (result.IsSuccess)
                Console.WriteLine($"Current owner: " + result.Data);
            else
                Console.WriteLine("Can't get owner: " + result.Error);

            var nftInfo = await sip09.GetById(nftId);

            Console.WriteLine($"NFT Name: '{nftInfo.Name}'");
        }

        static public async Task Demo()
        {
            await GetNFTData("SP213KNHB5QD308TEESY1ZMX1BP8EZDPG4JWD0MEA.web4::digital-land", 1);
            await GetNFTData("SP213KNHB5QD308TEESY1ZMX1BP8EZDPG4JWD0MEA.web4::digital-land", 2);
            await GetNFTData("SP213KNHB5QD308TEESY1ZMX1BP8EZDPG4JWD0MEA.web4::digital-land", 30);
        }
    }
}

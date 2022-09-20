using ChainAbstractions.Stacks;
using ShopSample;

// Fill with valid mnemonics
// See shop.md for details
string adminMnemonic = "almost solve purity lemon giggle wheel vast raccoon brown mistake tissue route hurdle dune sheriff give industry prison south subway fiber awkward desk vessel";
string clientMnemonic = "relax feature mammal chimney mule naive use damp ritual student flight polar brave alpha inner fit toy step grit weather ahead connect awake south";

if (string.IsNullOrEmpty(adminMnemonic) || string.IsNullOrEmpty(clientMnemonic))
{
    Console.WriteLine("Please setup admin and client mnemonics, see shop.md for details");
    return;
}

var server = new Server(StacksAbstractions.TestNet, adminMnemonic);
var client = new Client(StacksAbstractions.TestNet, clientMnemonic, server.Address, server.FullTokenId, server.FullNFTId);

Console.WriteLine("All blockchain transactions might take some considerable time, especially on the TestNet");
Console.WriteLine("Read more about transactions at:");
Console.WriteLine("https://docs.stacks.co/docs/understand-stacks/transactions");
Console.WriteLine("https://support.gamma.io/hc/en-us/articles/6011123313683-Why-does-it-take-a-few-minutes-for-my-transaction-to-be-confirmed-");

// await ConfigureServer();

Console.WriteLine("Trying to buy an NFT for game tokens...");
await client.BuyNFT(2, 100);

Console.WriteLine("Trying to fetch client nfts...");
await client.PrintOwnedNFTs();

async Task ConfigureServer()
{
    Console.WriteLine("Configuring smart contracts...");
    await server.Configure();

    Console.WriteLine("Sending some tokens to the client...");
    await server.GiveTokensTo(client.Address, 1300);
}


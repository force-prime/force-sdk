namespace StacksForce.Stacks
{
    public class Blockchain
    {
        public string Name { get; private set; }

        public string Endpoint { get; private set; }
        public string Websocket { get; private set; }

        public ChainID ChainId { get; private set; }

        public Blockchain(string name, string endpoint, ChainID chainId)
        {
            Name = name;
            Endpoint = $"https://{endpoint}/";
            Websocket = $"wss://{endpoint}/extended/v1/ws";
            ChainId = chainId;
        }

        public AddressVersion GetAddressVersion(bool isMultiSig = false)
        {
            if (this.ChainId == ChainID.Testnet)
                return isMultiSig ? AddressVersion.TestnetMultiSig : AddressVersion.TestnetSingleSig;
            else
                return isMultiSig ? AddressVersion.MainnetMultiSig : AddressVersion.MainnetSingleSig;
        }
    }

    public static class Blockchains
    {
        public static readonly Blockchain Mainnet = new Blockchain("Mainnet", "stacks-node-api.mainnet.stacks.co", ChainID.Mainnet);
        public static readonly Blockchain Testnet = new Blockchain("Testnet", "stacks-node-api.testnet.stacks.co", ChainID.Testnet);
    }
}

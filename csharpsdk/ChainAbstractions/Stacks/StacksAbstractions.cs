using StacksForce.Stacks;
using System.Collections.Generic;

namespace ChainAbstractions.Stacks
{
    static public partial class StacksAbstractions
    {
        static public readonly IBlockchain MainNet;
        static public readonly IBlockchain TestNet;

        static public IFungibleTokenData Stx = new StxTokenData();

        static public IBlockchain FromAddress(string address)
        {
            if (address.StartsWith("ST"))
                return TestNet;
            return MainNet;
        }

        static private FTMetaDataCache? FTCache(this Blockchain chain) => (_chain2Wrapper[chain] as BlockchainImplBasic).FTCache;

        static private readonly Dictionary<Blockchain, IBlockchain> _chain2Wrapper;

        static StacksAbstractions()
        {
            MainNet = new MainNetImpl();
            TestNet = new TestNetImpl();

            _chain2Wrapper = new Dictionary<Blockchain, IBlockchain>
            {
                {Blockchains.Mainnet, MainNet },
                {Blockchains.Testnet, TestNet },
            };

            StacksDependencies.SetupDefault();
        }
    }
}

using StacksForce.Stacks;
using System;
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

        static public string BalanceFormatted(this IFungibleToken token)
        {
            return token.Data.FormatCount(token.Balance);
        }

        static public IFungibleToken From(this IFungibleTokenData data, double value)
        {
            var ftData = (FungibleTokenData)data;
            return new FungibleToken((ulong)(value * Math.Pow(10, ftData.Decimals)), ftData);
        }

        static public string FormatBalance(ulong count, uint decimals, string code)
        {
            if (decimals == 0)
                return count.ToString() + " " + code;

            var lowPartSize = (ulong)Math.Pow(10, decimals);
            var mainPart = count / lowPartSize;
            var fracPart = count % lowPartSize;
            return mainPart + "." + fracPart + " " + code;
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

            if (StacksForce.Dependencies.DependencyProvider.Cryptography == null)
                StacksDependencies.SetupDefault();
        }
    }
}

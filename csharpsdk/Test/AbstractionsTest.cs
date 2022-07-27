using StacksForce.Stacks;

namespace StacksForceTest
{
    public class AbstractionsTest
    {
        static AbstractionsTest()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public async void TestWalletInfo()
        {
            var walletInfo = StacksAbstractions.TestNet.GetWalletInfoForAddress("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ");
            var stx = await walletInfo.GetToken(null).ConfigureAwait(false);
            Assert.True(stx != null);
            Assert.Equal("499.880036 STX", stx.BalanceFormatted());
            var nftStream = walletInfo.GetNFTs(null);
            var nfts = await nftStream.ReadMoreAsync(1).ConfigureAwait(false);
            Assert.Equal("TEST-NFT", nfts[0].Name);
        }
    }
}

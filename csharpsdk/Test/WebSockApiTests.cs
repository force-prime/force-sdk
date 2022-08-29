using StacksForce.Stacks;
using StacksForce.Stacks.WebApi;

namespace StacksForceTest
{
    public class WebSockApiTests
    {
        [Fact]
        public async void SubscribeToBlock()
        {
            var w = new WebSocketAPI(Blockchains.Mainnet);
            w.Connect(false);

            var error = await w.SubsribeToBlockEvent();
            Assert.Null(error);

            error = await w.UnsubsribeToBlockEvent();
            Assert.Null(error);
        }

        [Fact]
        public async void SubscribeToTransactionUpdateIncorrect()
        {
            var w = new WebSocketAPI(Blockchains.Mainnet);
            w.Connect(false);

            var error = await w.SubsribeToTransactionUpdate("incorrect-address");
            Assert.Equal("Invalid params", error.Id);
        }

        [Fact]
        public async void SubscribeToAddress()
        {
            var w = new WebSocketAPI(Blockchains.Mainnet);
            w.Connect(false);

            var error = await w.SubsribeToAddressTransactions("SP384CVPNDTYA0E92TKJZQTYXQHNZSWGCAG7SAPVB");
            Assert.Null(error);

            error = await w.SubsribeToBalanceUpdate("SP384CVPNDTYA0E92TKJZQTYXQHNZSWGCAG7SAPVB");
            Assert.Null(error);

            error = await w.SubsribeToBalanceUpdate("incorrect-addess");
            Assert.Equal("Invalid params", error.Id);

            error = await w.UnsubsribeToAddressTransactions("SP384CVPNDTYA0E92TKJZQTYXQHNZSWGCAG7SAPVB");
            Assert.Null(error);

            error = await w.UnsubsribeToBalanceUpdate("SP384CVPNDTYA0E92TKJZQTYXQHNZSWGCAG7SAPVB");
            Assert.Null(error);
        }
    }
}

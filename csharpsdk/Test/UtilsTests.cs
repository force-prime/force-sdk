using StacksForce.Utils;

namespace StacksForceTest
{
    public class UtilsTests
    {
        #region required classes
        public enum TestEnum
        {
            Val1,
            ValSecond,
            Undefined
        }

        public class TestDataStreamBase : BasicDataStream<int>
        {
            protected async override Task<List<int>> GetRange(long index, long count)
            {
                await Task.Delay(1);
                return Enumerable.Range((int) index, (int) count).ToList();
            }
            protected override Task? Prepare()
            {
                return Task.Delay(100);
            }
        }

        #endregion

        [Fact]
        public static void TestEnumUtils()
        {
            var v = EnumUtils.FromString("VAL1", TestEnum.Undefined);
            Assert.Equal(TestEnum.Val1, v);
            v = EnumUtils.FromString("val_second", TestEnum.Undefined);
            Assert.Equal(TestEnum.ValSecond, v);
            v = EnumUtils.FromString("Val2", TestEnum.Undefined);
            Assert.Equal(TestEnum.Undefined, v);
        }

        [Fact]
        public static async void TestDataStream()
        {
            var stream = new TestDataStreamBase();
            for (int i = 0; i < 10; i++)
            {
                var data = await stream.ReadMoreAsync(1).ConfigureAwait(false);
                Assert.Equal(i, data[0]);
            }
        }

        [Fact]
        public static void TestBuildUrl()
        {
            string expected = "https://test.test/?vint=22&vstr=string&arr=1%2c2%2c3&bool=False";
            Dictionary<string, object?> fields = new Dictionary<string, object?>()
            {
                {"vint", 22 },
                {"vstr", "string" },
                {"arr", new int[]  {1, 2, 3}},
                {"bool", false }
            };

            var url = HttpHelper.BuildUrl("https://test.test/", fields);
            Assert.Equal(expected, url);
        }

        [Fact]
        public static void TestByteUtils()
        {
            string hex = "0xabcdef";
            byte[] bytes = new byte[] { 0xab, 0xcd, 0xef };
            Assert.Equal(hex.ToHexByteArray(), bytes);
            Assert.Equal("0x" + bytes.ToHex(), hex);
            Assert.Equal(bytes.Slice(1), new byte[] { 0xcd, 0xef });
            Assert.Equal(bytes.Slice(-2), new byte[] { 0xcd, 0xef });
            Assert.Equal(bytes.PadLeft(5, 0), new byte[] { 0, 0, 0xab, 0xcd, 0xef });
            Assert.Equal(bytes.TrimEnd(), bytes);
        }
    }
}

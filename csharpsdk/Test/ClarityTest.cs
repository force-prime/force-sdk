using StacksForce.Stacks;
using System.Numerics;

namespace StacksForceTest
{
    public class ClarityTest
    {
        [Fact]
        public void TestUint()
        {
            (string hex, BigInteger value)[] tests = new (string hex, BigInteger value)[]
            {
                ("0100000000000000000000000001701adc", 24124124),
                ("0100000000000000000000000000000000", 0),
                ("0100000001a3870e751d2bf86b220623be", FromStr("129837492374293422323433612222")),
            };

            foreach (var test in tests)
            {
                var value = Clarity.Value.FromHex(test.hex);
                Assert.True(value is Clarity.UInteger128);
                Assert.Equal(test.value, (value as Clarity.UInteger128).Value);
                Assert.Equal(test.hex, (value as Clarity.UInteger128).AsHex());
            }
        }

        [Fact]
        public void TestInt()
        {
            (string hex, BigInteger value)[] tests = new (string hex, BigInteger value)[]
            {
                ("00ffffffffffffffffffffffffffffffff", -1),
                ("00fffffffe5c78f18ae2d40794ddf9dc42", FromStr("-129837492374293422323433612222")),
                ("0000000001a3870e751d2bf86b220623be", FromStr("129837492374293422323433612222")),
            };

            foreach (var test in tests)
            {
                var value = Clarity.Value.FromHex(test.hex);
                Assert.True(value is Clarity.Integer128);
                Assert.Equal(test.value, (value as Clarity.Integer128).Value);
                Assert.Equal(test.hex, (value as Clarity.Integer128).AsHex());
            }
        }

        [Fact]
        public void TestBool()
        {
            (string hex, bool value)[] tests = new (string hex, bool value)[]
            {
                ("03", true),
                ("04", false),
            };

            foreach (var test in tests)
            {
                var value = Clarity.Value.FromHex(test.hex);
                Assert.True(value is Clarity.Boolean);
                Assert.Equal(test.value, (value as Clarity.Boolean).Value);
                Assert.Equal(test.hex, (value as Clarity.Boolean).AsHex());
            }
        }

        [Fact]
        public void TestNone()
        {
            var value = Clarity.Value.FromHex("09");
            Assert.True(value is Clarity.None);
            Assert.Equal("09", value.AsHex());
        }


        [Fact]
        public void TestStrings()
        {
            (string hex, string value)[] tests = new (string hex, string value)[]
            {
                ("0d0000000b68656c6c6f20776f726c64", "hello world"),
                ("0e0000000a68656c6c6f20f09f8cbe", "hello 🌾"),
            };

            foreach (var test in tests)
            {
                var value = Clarity.Value.FromHex(test.hex);
                Assert.True(value is Clarity.StringType);
                Assert.Equal(test.value, (value as Clarity.StringType).Value);
                Assert.Equal(test.hex, (value as Clarity.StringType).AsHex());
            }
        }

        [Fact]
        public void TestBuffer()
        {
            string hex = "0200000004deadbeef";
            var value = Clarity.Value.FromHex(hex);
            Assert.True(value is Clarity.ByteBuffer);
            Assert.Equal(new byte[] { 0xde, 0xad, 0xbe, 0xef }, (value as Clarity.ByteBuffer).Value);
            Assert.Equal(hex, value.AsHex());
        }

        [Fact]
        public void TestSome()
        {
            string hex = "0a00ffffffffffffffffffffffffffffffff";
            var value = Clarity.Value.FromHex(hex);
            Assert.True(value is Clarity.OptionalSome);
            Assert.Equal(value.UnwrapUntil<Clarity.Integer128>().Value, -1);
            Assert.Equal(hex, value.AsHex());
        }

        [Fact]
        public void TestList()
        {
            var serialized = "0b00000003030e0000000a68656c6c6f20f09f8cbe04";
            var value = Clarity.Value.FromHex(serialized);
            Assert.True(value is Clarity.List);
            var list = (value as Clarity.List).Values;
            Assert.Equal(true, (list[0] as Clarity.Boolean).Value);
            Assert.Equal("hello 🌾", (list[1] as Clarity.StringType).Value);
            Assert.Equal(false, (list[2] as Clarity.Boolean).Value);
            Assert.Equal(serialized, value.AsHex());
        }

        [Fact]
        public void TestPrincipal()
        {
            var serialized = "0516a5d9d331000f5b79578ce56bd157f29a9056f0d6";
            var value = Clarity.Value.FromHex(serialized);
            Assert.True(value is Clarity.StandardPrincipal);
            Assert.Equal("SP2JXKMSH007NPYAQHKJPQMAQYAD90NQGTVJVQ02B", (value as Clarity.StandardPrincipal).Address);
            Assert.Equal(serialized, value.AsHex());
        }

        [Fact]
        public void TestContractPrincipal()
        {
            var serialized = "0616a5d9d331000f5b79578ce56bd157f29a9056f0d60d746573742d636f6e7472616374";
            var value = Clarity.Value.FromHex(serialized);
            Assert.True(value is Clarity.ContractPrincipal);
            Assert.Equal("SP2JXKMSH007NPYAQHKJPQMAQYAD90NQGTVJVQ02B", (value as Clarity.ContractPrincipal).Address);
            Assert.Equal("test-contract", (value as Clarity.ContractPrincipal).Contract);
            Assert.Equal(serialized, value.AsHex());
        }

        [Fact]
        public void TestTuple()
        {
            var serialized = "0c00000003096e616d6573706163650200000003666f6f0a70726f706572746965730c000000061963616e2d7570646174652d70726963652d66756e6374696f6e030b6c61756e636865642d61740a0100000000000000000000000000000006086c69666574696d65010000000000000000000000000000000c106e616d6573706163652d696d706f7274051a164247d6f2b425ac5771423ae6c80c754f7172b00e70726963652d66756e6374696f6e0c0000000504626173650100000000000000000000000000000001076275636b6574730b00000010010000000000000000000000000000000101000000000000000000000000000000010100000000000000000000000000000001010000000000000000000000000000000101000000000000000000000000000000010100000000000000000000000000000001010000000000000000000000000000000101000000000000000000000000000000010100000000000000000000000000000001010000000000000000000000000000000101000000000000000000000000000000010100000000000000000000000000000001010000000000000000000000000000000101000000000000000000000000000000010100000000000000000000000000000001010000000000000000000000000000000105636f6566660100000000000000000000000000000001116e6f2d766f77656c2d646973636f756e740100000000000000000000000000000001116e6f6e616c7068612d646973636f756e7401000000000000000000000000000000010b72657665616c65642d61740100000000000000000000000000000003067374617475730d000000057265616479";
            var value = Clarity.Value.FromHex(serialized);
            Assert.True(value is Clarity.Tuple);
            var dict = (value as Clarity.Tuple).Values;
            Assert.Equal("ready", dict["status"].UnwrapUntil<Clarity.StringType>().Value);
            Assert.Equal(serialized, value.AsHex());
        }


        private static BigInteger FromStr(string str)
        {
            BigInteger.TryParse(str, out var result);
            return result;
        }
    }
}

using StacksForce.Utils;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace StacksForce.Stacks
{
    public static class Clarity
    {
        public enum Types
        {
            Int = 0x00,
            UInt = 0x01,
            Buffer = 0x02,
            BoolTrue = 0x03,
            BoolFalse = 0x04,
            PrincipalStandard = 0x05,
            PrincipalContract = 0x06,
            ResponseOk = 0x07,
            ResponseErr = 0x08,
            None = 0x09,
            OptionalSome = 0x0a,
            List = 0x0b,
            Tuple = 0x0c,
            StringASCII = 0x0d,
            StringUTF8 = 0x0e,
        }

        public abstract class Value : IBinarySerializable
        {
            private readonly Types _type;
            public Types Type => _type;

            protected Value(Types type)
            {
                _type = type;
            }

            public virtual void SerializeTo(BinaryWriter writer)
            {
                writer.Write((byte)_type);
            }

            public string AsHex() => this.ToHexString();

            static public Value? FromBytes(byte[] bytes)
            {
                var valueBytes = bytes.Slice(1);
                switch (bytes[0])
                {
                    case (byte)Types.ResponseErr:
                        return new Err(FromBytes(valueBytes));
                    case (byte)Types.ResponseOk:
                        return new Ok(FromBytes(valueBytes));
                    case (byte)Types.None:
                        return new None();
                    case (byte)Types.UInt:
                        return new UInteger128(valueBytes);
                    case (byte)Types.StringASCII:
                        return new StringType(valueBytes.Slice(4), Encoding.ASCII, Types.StringASCII); // skip 4 len bytes
                    case (byte)Types.OptionalSome:
                        return new OptionalSome(FromBytes(valueBytes));
                    case (byte)Types.PrincipalStandard:
                        return StandardPrincipal.Deserialize(valueBytes);
                    case (byte)Types.PrincipalContract:
                        return ContractPrincipal.Deserialize(valueBytes);
                    case (byte)Types.BoolTrue:
                    case (byte)Types.BoolFalse:
                        return new Boolean(bytes[0] == (byte)Types.BoolTrue);
                    case (byte)Types.Tuple:
                        return Tuple.FromBytes(valueBytes);
                }
                return null;
            }

            static public Value FromHex(string hex) {
                var bytes = hex.ToHexByteArray();
                return FromBytes(bytes);
            }
        }

        public class None : Value
        {
            public None() : base(Types.None) { }
            public override string ToString()
            {
                return "none";
            }
        }

        public class Boolean : Value
        {
            private readonly bool _value;
            public bool Value => _value;
            public Boolean(bool value) : base(value ? Types.BoolTrue : Types.BoolFalse)
            {
                _value = value;
            }

            public override string ToString() => _value.ToString();
        }

        public class Tuple : Value
        {
            private readonly Dictionary<string, Value> _values;

            public Tuple(Dictionary<string, Value> values) : base(Types.Tuple)
            {
                _values = values;
            }

            static public Tuple FromBytes(byte[] bytes)
            {
              //  var length = SerializationUtils.DeserializeUInt32BE(bytes.Slice(0, 4));
                var content = bytes.Slice(4); // skip length
                return null;
            }
        }

        public class WrappedValue : Value
        {
            public Value Value { get; }
            public WrappedValue(Value value, Types type) : base(type)
            {
                Value = value;
            }

            public override string ToString()
            {
                return $"({GetType().Name} {Value})";
            }
        }

        public class OptionalSome : WrappedValue
        {
            public OptionalSome(Value value) : base(value, Types.OptionalSome) { }
        }

        public class Ok : WrappedValue
        {
            public Ok(Value value) : base(value, Types.ResponseOk) { }
        }

        public class Err : WrappedValue
        {
            public Err(Value value) : base(value, Types.ResponseErr) { }
        }

        public class ByteBuffer : Value
        {
            public byte[] bytes;

            public ByteBuffer(byte[] bytes) : base(Types.Buffer) {
                this.bytes = bytes;
            }

            public ByteBuffer(string hexString) : base(Types.Buffer)
            {
                this.bytes = ByteUtils.ToHexByteArray(hexString);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(ByteUtils.UInt32ToByteArray((uint) bytes.Length));
                writer.Write(bytes);
            }
        }

        public class UInteger128 : Value
        {
            public BigInteger value;

            public UInteger128(ulong v) : base(Types.UInt)
            {
                value = v;
            }
            public UInteger128(byte[] v) : base(Types.UInt)
            {
                value = new BigInteger(v, true, true);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeBigInteger(value));
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        public abstract class Principal : Value
        {
            protected Principal(Types type) : base(type) { }

            static public Principal FromString(string address)
            {
                if (address.Contains("."))
                {
                    var addressAndContract = address.Split('.');
                    return new ContractPrincipal(addressAndContract[0], addressAndContract[1]);
                }

                return new StandardPrincipal(address);
            }
        }

        public class StandardPrincipal : Principal
        {
            private readonly string _address;

            public StandardPrincipal(string address) : base(Types.PrincipalStandard)
            {
                this._address = address;
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeAddress(_address));
            }

            static public StandardPrincipal? Deserialize(byte[] bytes)
            {
                var address = SerializationUtils.DeserializeAddress(bytes.Slice(0, 21));
                if (address == null)
                    return null;
                return new StandardPrincipal(address);
            }

            public override string ToString() => _address;
        }

        public class ContractPrincipal : Principal
        {
            private readonly string _address;
            private readonly string _contract;

            public ContractPrincipal(string address, string contract) : base(Types.PrincipalContract)
            {
                _address = address;
                _contract = contract;
            }
            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeAddress(_address));
                writer.Write(Encoding.ASCII.GetBytes(_contract));
            }

            static public ContractPrincipal? Deserialize(byte[] bytes)
            {
                var address = SerializationUtils.DeserializeAddress(bytes.Slice(0, 21));
                if (address == null)
                    return null;
                var contract = Encoding.ASCII.GetString(bytes.Slice(22));
                return new ContractPrincipal(address, contract);
            }

            public override string ToString() => $"{_address}.{_contract}";
        }

        public class StringType : Value
        {
            private string _str;

            public string Str => _str;

            public StringType(byte[] bytes, Encoding encoding, Types type) : base(Types.StringASCII)
            {
                _str = encoding.GetString(bytes);
            }

            public override string ToString() => _str;
        }
    }

    public static class ClarityUtils
    {
        public static bool IsOk(this Clarity.Value value) => value is Clarity.Ok;
        public static T? UnwrapUntil<T>(this Clarity.Value value) where T : Clarity.Value
        {
            if (value is T asT)
                return asT;
            if (value is Clarity.WrappedValue wrapped)
                return wrapped.Value.UnwrapUntil<T>();
            return null;
        }
    }
}

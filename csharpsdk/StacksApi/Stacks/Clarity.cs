using StacksForce.Utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            protected readonly Types _type;
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

            static public Value? FromBytes(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                ulong valueReadCount = bytes[0] switch
                {
                    (byte)Types.UInt => 16,
                    (byte)Types.Int => 16,
                    _ => 0
                };

                var valueBytes = bytes.Slice(1);

                Value? result = bytes[0] switch
                {
                    (byte)Types.Buffer => ByteBuffer.ReadFrom(valueBytes, out valueReadCount),
                    (byte)Types.ResponseErr => new Err(FromBytes(valueBytes, out valueReadCount)),
                    (byte)Types.ResponseOk => new Ok(FromBytes(valueBytes, out valueReadCount)),
                    (byte)Types.None => new None(),
                    (byte)Types.UInt => new UInteger128(valueBytes.Slice(0, 16)),
                    (byte)Types.Int => new Integer128(valueBytes.Slice(0, 16)),
                    (byte)Types.StringASCII => StringType.ReadFrom(valueBytes, Encoding.ASCII, Types.StringASCII, out valueReadCount),
                    (byte)Types.StringUTF8 => StringType.ReadFrom(valueBytes, Encoding.UTF8, Types.StringUTF8, out valueReadCount),
                    (byte)Types.OptionalSome => new OptionalSome(FromBytes(valueBytes, out valueReadCount)),
                    (byte)Types.PrincipalStandard => StandardPrincipal.ReadFrom(valueBytes, out valueReadCount),
                    (byte)Types.PrincipalContract => ContractPrincipal.ReadFrom(valueBytes, out valueReadCount),
                    (byte)Types.BoolTrue => new Boolean(true),
                    (byte)Types.BoolFalse => new Boolean(false),
                    (byte)Types.Tuple => Tuple.ReadFrom(valueBytes, out valueReadCount),
                    (byte)Types.List => List.ReadFrom(valueBytes, out valueReadCount),
                };

                readCount = 1 + valueReadCount;

                return result;
            }

            static public Value? FromHex(string hex) {
                var bytes = hex.ToHexByteArray();
                return FromBytes(bytes, out var _);
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

        public class List : Value
        {
            private readonly Value[] _values;

            public Value[] Values => _values;

            public List(params Value[] values) : base(Types.List)
            {
                _values = values;
            }

            static public List ReadFrom(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                var values = new List<Value>();

                var len = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                int position = 4;
                for (int i = 0; i < len; i++)
                {
                    Value? cv = Clarity.Value.FromBytes(bytes.Slice(position), out var byteCount);
                    position += (int)byteCount;
                    values.Add(cv);
                }

                readCount = (ulong)position;

                return new List(values.ToArray());
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(ByteUtils.UInt32ToByteArrayBigEndian((uint)_values.Length));
                for (int i = 0; i < _values.Length; i++)
                    _values[i].SerializeTo(writer);
            }

            public override string ToString()
            {
                return $"[{string.Join<Value>(",", _values)}]";
            }
        }

        public class Tuple : Value
        {
            private readonly Dictionary<string, Value> _values;

            public IReadOnlyDictionary<string, Value> Values => _values;

            public Tuple(Dictionary<string, Value> values) : base(Types.Tuple)
            {
                _values = values;
            }

            static public Tuple ReadFrom(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                var dict = new Dictionary<string, Value>();

                var len = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                int position = 4;
                for (int i = 0; i < len; i++)
                {
                    var nameLen = bytes[position];
                    position++;
                    var name = Encoding.ASCII.GetString(bytes.Slice(position, nameLen));
                    position += nameLen;
                    Value? cv = Value.FromBytes(bytes.Slice(position), out var byteCount);
                    position += (int) byteCount;
                    dict.Add(name, cv);
                }

                readCount = (ulong) position;

                return new Tuple(dict);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(ByteUtils.UInt32ToByteArrayBigEndian((uint)_values.Count));

                foreach (var keyAndValue in _values.OrderBy(x=>x.Key))
                {
                    writer.Write(SerializationUtils.SerializeLPString(keyAndValue.Key));
                    keyAndValue.Value.SerializeTo(writer);
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("{");
                foreach (var keyAndValue in _values)
                {
                    sb.Append(keyAndValue.Key);
                    sb.Append(": ");
                    sb.Append(keyAndValue.Value.ToString());
                    sb.Append(", ");
                }
                sb.Append("}");
                return sb.ToString();
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

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                Value.SerializeTo(writer);
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
            private readonly byte[] _bytes;

            public byte[] Value => _bytes;

            public ByteBuffer(byte[] bytes) : base(Types.Buffer) {
                _bytes = bytes;
            }

            public ByteBuffer(string hexString) : base(Types.Buffer)
            {
                _bytes = ByteUtils.ToHexByteArray(hexString);
            }

            static public ByteBuffer ReadFrom(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                var len = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                readCount = 4 + len;
                return new ByteBuffer(bytes.Slice(4, (int)len).ToArray());
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(ByteUtils.UInt32ToByteArrayBigEndian((uint)_bytes.Length));
                writer.Write(_bytes);
            }

            public override string ToString()
            {
                return "0x" + _bytes.ToHex();
            }
        }

        public class UInteger128 : Value
        {
            private BigInteger _value;

            public BigInteger Value => _value;

            public UInteger128(ulong v) : base(Types.UInt)
            {
                _value = v;
            }

            public UInteger128(BigInteger value) : base(Types.UInt)
            {
                _value = value;
            }

            public UInteger128(ReadOnlySpan<byte> bytes) : base(Types.UInt)
            {
                _value = new BigInteger(bytes, true, true);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeBigUInteger(_value));
            }

            public override string ToString()
            {
                return _value.ToString();
            }
        }

        public class Integer128 : Value
        {
            private readonly BigInteger _value;

            public BigInteger Value => _value;

            public Integer128(long v) : base(Types.Int)
            {
                _value = v;
            }
            public Integer128(ReadOnlySpan<byte> bytes) : base(Types.Int)
            {
                _value = new BigInteger(bytes, false, true);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);

                writer.Write(ByteUtils.Int128ToByteArrayBigEndian(_value));
            }

            public override string ToString()
            {
                return _value.ToString();
            }
        }

        public abstract class Principal : Value
        {
            protected Principal(Types type) : base(type) { }

            static public Principal FromString(string address)
            {
                if (address.Contains("."))
                {
                    var addressAndContract = address.Split(".");
                    return new ContractPrincipal(addressAndContract[0], addressAndContract[1]);
                }

                return new StandardPrincipal(address);
            }
        }

        public class StandardPrincipal : Principal
        {
            private readonly string _address;

            public string Address => _address;

            public StandardPrincipal(string address) : base(Types.PrincipalStandard)
            {
                this._address = address;
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeAddress(_address));
            }

            static public StandardPrincipal? ReadFrom(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                readCount = 21;
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

            public string Address => _address;
            public string Contract => _contract;

            public ContractPrincipal(string address, string contract) : base(Types.PrincipalContract)
            {
                _address = address;
                _contract = contract;
            }
            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);
                writer.Write(SerializationUtils.SerializeAddress(_address));
                writer.Write(SerializationUtils.SerializeLPString(_contract));
            }

            static public ContractPrincipal? ReadFrom(ReadOnlySpan<byte> bytes, out ulong readCount)
            {
                var address = SerializationUtils.DeserializeAddress(bytes.Slice(0, 21));
                var contractLen = bytes[21];
                var contract = Encoding.ASCII.GetString(bytes.Slice(22, contractLen));
                readCount = 22 + (ulong) contractLen;

                return new ContractPrincipal(address, contract);
            }

            public override string ToString() => $"{_address}.{_contract}";
        }

        public class StringType : Value
        {
            private readonly string _str;
            public string Value => _str;

            public StringType(string str, Types type) : base(type)
            {
                _str = str;
            }

            public override string ToString() => _str;

            public static StringType ReadFrom(ReadOnlySpan<byte> bytes, Encoding enc, Types valueType, out ulong readCount)
            {
                var len = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                var str = enc.GetString(bytes.Slice(4, (int) len));
                readCount = 4 + len;
                return new StringType(str, valueType);
            }

            public override void SerializeTo(BinaryWriter writer)
            {
                base.SerializeTo(writer);

                Encoding enc = _type == Types.StringASCII ? Encoding.ASCII : Encoding.UTF8;
                var bytes = enc.GetBytes(_str);

                writer.Write(ByteUtils.UInt32ToByteArrayBigEndian((uint)bytes.Length));
                writer.Write(bytes);
            }
        }
    }

    public static class ClarityUtils
    {
        public static bool IsOk(this Clarity.Value value) => value is Clarity.Ok;
        public static bool IsErr(this Clarity.Value value) => value is Clarity.Err;
        public static bool IsNone(this Clarity.Value value) => value is Clarity.None;

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

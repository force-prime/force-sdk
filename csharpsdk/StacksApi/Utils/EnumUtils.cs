using System;
using System.Collections.Generic;
using System.Linq;

namespace StacksForce.Utils
{
    static public class EnumUtils
    {
        static public readonly string[] DEFAULT_SKIP_SYMBOLS = new string[] { "_" };

        static public List<string> FromEnum<TEnum>() {
            return Enum.GetNames(typeof(TEnum)).ToList();
        }

        static public TEnum FromString<TEnum>(string value, TEnum defaultValue, string[]? skipSybmols = null) where TEnum: struct
        {
            skipSybmols = skipSybmols ?? DEFAULT_SKIP_SYMBOLS;

            Array.ForEach(skipSybmols, c => value = value.Replace(c, string.Empty));

            if (Enum.TryParse<TEnum>(value, true, out TEnum result))
                return result;

            return defaultValue;
        }
    }
}

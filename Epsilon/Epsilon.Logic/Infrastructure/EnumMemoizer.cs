using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Epsilon.Logic.Infrastructure
{
    public class EnumMemoizer<T> where T : struct, IConvertible
    {
        private readonly ImmutableList<T> _values;
        private readonly ImmutableList<string> _names;
        // Key is stored as all lowercase
        private readonly ImmutableDictionary<string, T> _fromString;
        // Value is stored in whatever case is returned by ToString on the enum.
        private readonly ImmutableDictionary<T, string> _toString;

        public EnumMemoizer()
        {
            var enumType = typeof(T);
            var enumOptions = (Enum.GetValues(enumType) as T[]);
            _values = enumOptions.ToImmutableList();
            _names = Enum.GetNames(enumType).ToImmutableList();
            _fromString = enumOptions.ToImmutableDictionary(en => en.ToString().ToLower(), en => en);
            _toString = enumOptions.ToImmutableDictionary(en => en, en => en.ToString());
        }

        public List<T> GetValues()
        {
            return _values.ToList();
        }

        public List<string> GetNames()
        {
            return _names.ToList();
        }

        public T? Parse(string value)
        {
            T answer;
            if (_fromString.TryGetValue(value.ToLower(), out answer))
                return answer;
            return null;
        }

        public string ToString(T value)
        {
            return _toString[value];
        }
    }
}

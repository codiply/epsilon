using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Epsilon.Logic.Infrastructure
{
    public class EnumMemoizer<T> where T : struct, IConvertible
    {
        // Key is stored as all lowercase
        private readonly ImmutableDictionary<string, T> _fromString;
        // Value is stored in whatever case is returned by ToString on the enum.
        private readonly ImmutableDictionary<T, string> _toString;

        public EnumMemoizer()
        {
            var enumOptions = (Enum.GetValues(typeof(T)) as T[]);
            _fromString = enumOptions.ToImmutableDictionary(en => en.ToString().ToLower(), en => en);
            _toString = enumOptions.ToImmutableDictionary(en => en, en => en.ToString());
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

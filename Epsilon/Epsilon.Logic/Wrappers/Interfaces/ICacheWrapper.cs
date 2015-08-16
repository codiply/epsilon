using System;
using System.Collections.Generic;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface ICacheWrapper
    {
        bool ContainsKey(string key);

        Object Get(string key);

        void Insert(string key, Object value);
        void Insert(string key, Object value, TimeSpan slidingExpiration);

        void Remove(string key);

        IEnumerable<string> AllKeys();

        void Clear();
    }
}

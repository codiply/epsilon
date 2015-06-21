using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Infrastructure.Interfaces
{
    public enum WithLock
    {
        Yes, No
    }

    public interface IAppCache
    {
        bool ContainsKey(string key);

        T Get<T>(string key, Func<T> getItemCallback, WithLock lockOption) where T : class;

        T Get<T>(string key, Func<T> getItemCallback, TimeSpan slidingExpiration, WithLock lockOption) where T : class;

        T Get<T>(string key, Func<T> getItemCallback, DateTime absoluteExpiration, WithLock lockOption) where T : class;
        
        void Remove(string key);

        IEnumerable<string> AllKeys();

        void Clear();
    }
}

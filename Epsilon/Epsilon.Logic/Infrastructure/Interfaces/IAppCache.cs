using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Infrastructure.Interfaces
{
    public interface IAppCache
    {
        bool ContainsKey(string key);

        T Get<T>(string key, Func<T> getItemCallback) where T : class;

        T Get<T>(string key, Func<T> getItemCallback, TimeSpan slidingExpiration) where T : class;

        T Get<T>(string key, Func<T> getItemCallback, DateTime absoluteExpiration) where T : class;

        Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback) where T : class;

        Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback, TimeSpan slidingExpiration) where T : class;

        Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback, DateTime absoluteExpiration) where T : class;

        void Remove(string key);

        IEnumerable<string> AllKeys();

        void Clear();
    }
}

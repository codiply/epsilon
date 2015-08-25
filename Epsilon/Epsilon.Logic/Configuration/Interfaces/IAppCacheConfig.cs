namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAppCacheConfig
    {
        bool DisableAppCache { get; }
        bool DisableAsynchronousLocking { get; }
        bool DisableSynchronousLocking { get; }
    }
}

using System;
using System.Threading.Tasks;

namespace Dan.Pseudo.Services.Interfaces;

public interface IPluginMemoryCacheProvider
{
    public Task<(bool success, T result)> TryGet<T>(string key);

    public Task<T> SetCache<T>(string key, T model, TimeSpan timeToLive);
}


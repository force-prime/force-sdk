using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    public class CachedDictionaryAsync<TKey, TValue> 
    {
        private bool _dropCacheOnLoad = false;
        private readonly Dictionary<TKey, Data> _cachedData = new Dictionary<TKey, Data>();

        private Func<TKey, object, Task<TValue>> _asyncGetter;

        public CachedDictionaryAsync(Func<TKey, object, Task<TValue>> asyncGetter)
        {
            _asyncGetter = asyncGetter;
        }

        public void SetDropCacheOnLoad()
        {
            _dropCacheOnLoad = true;
        }

        public bool GetIfContains(TKey id, out TValue value)
        {
            lock (_cachedData)
            {
                if (_cachedData.TryGetValue(id, out var data) && data.task == null)
                {
                    value = data.value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public async ValueTask<TValue> Get(TKey id, object additionalData = null)
        {
            Task<TValue> task;
            Data data;
            lock (_cachedData)
            {
                if (!_cachedData.TryGetValue(id, out data))
                {
                    data = new Data();
                    _cachedData.Add(id, data);
                    data.task = task = _asyncGetter(id, additionalData);
                } else
                {
                    if (data.task == null)
                        return data.value;
                    task = data.task;
                }
            }
            var value = await task.ConfigureAwait();
            data.value = value;
            data.task = null;
            if (_dropCacheOnLoad)
                DropCache(id);
            return value;
        }

        public void DropCache(TKey id)
        {
            lock (_cachedData)
            {
                _cachedData.Remove(id);
            }
        }

        private class Data
        {
            public TValue value;
            public Task<TValue>? task;
        }
    }
}
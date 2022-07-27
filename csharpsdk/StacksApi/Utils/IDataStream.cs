using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    public interface IDataStream<T>
    {
        Task<List<T>?> ReadMoreAsync(int count);
    }

    public abstract class BasicDataStream<T> : IDataStream<T>
    {
        private int _index = 0;

        private int _inRead = 0;

        private bool _prepared;

        protected BasicDataStream()
        {
        }

        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            if (Interlocked.Exchange(ref _inRead, 1) != 0)
                throw new InvalidOperationException();

            if (!_prepared)
            {
                _prepared = true;
                var prepareTask = Prepare();
                if (prepareTask != null)
                    await prepareTask.ConfigureAwait(false);
            }

            List<T> allItems = new List<T>();

            bool hasError = false;

            var items = await GetRange(_index, count).ConfigureAwait(false);
            if (items != null)
            {
                allItems.AddRange(items);
                _index += items.Count;
            }
            else
                hasError = true;

            _inRead = 0;

            if (hasError)
                return null;

            return allItems;
        }

        protected virtual Task? Prepare() => null;

        protected abstract Task<List<T>> GetRange(long index, long count);
    }
}

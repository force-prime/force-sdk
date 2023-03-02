using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace StacksForce.Utils
{
    public interface IDataStream<T>
    {
        Task<List<T>?> ReadMoreAsync(int count);
    }

    public interface IDataStreamProvider<T>
    {
        IDataStream<T> GetStream();
    }

    public sealed class EmptyDataStream<T> : IDataStream<T>
    {
        static public readonly EmptyDataStream<T> EMPTY = new EmptyDataStream<T>();

        static private readonly List<T> EMPTY_LIST = new List<T>(0);
        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            return EMPTY_LIST;
        }
    }

    public sealed class EmptyDataStreamProvider<T> : IDataStreamProvider<T>
    {
        static public readonly IDataStreamProvider<T> EMPTY = new EmptyDataStreamProvider<T>();
        public IDataStream<T> GetStream() => EmptyDataStream<T>.EMPTY;
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
            {
                Log.Fatal("ReadMoreAsync: incorrect read state");
                throw new InvalidOperationException();
            }

            if (!_prepared)
            {
                _prepared = true;
                var prepareTask = Prepare();
                if (prepareTask != null)
                    await prepareTask.ConfigureAwait();
            }

            List<T> allItems = new List<T>();

            bool hasError = false;

            var items = await GetRange(_index, count).ConfigureAwait();
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

        protected abstract Task<List<T>?> GetRange(long index, long count);
    }

    public class FilterDataStream<T> : IDataStream<T>
    {
        private readonly IDataStream<T> _stream;
        private readonly Predicate<T> _filter;
        private readonly List<T> _items = new List<T>();

        public FilterDataStream(IDataStream<T> source, Predicate<T> filter)
        {
            _stream = source;
            _filter = filter;
        }

        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            _items.Clear();
            while (_items.Count < count)
            {
                var data = await _stream.ReadMoreAsync(count - _items.Count).ConfigureAwait();
                if (data == null)
                    return null;

                if (data.Count == 0)
                    break;

                foreach (var item in data)
                    if (_filter(item))
                        _items.Add(item);
            }
            return _items;
        }
    }

    public class TransformDataStream<T, TSource> : IDataStream<T>
    {
        public delegate T TransformFunction(TSource source);

        private readonly IDataStream<TSource> _source;
        private readonly TransformFunction _transform;

        public TransformDataStream(IDataStream<TSource> source, TransformFunction transform)
        {
            _source = source;
            _transform = transform;
        }

        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            var data = await _source.ReadMoreAsync(count).ConfigureAwait();
            if (data == null)
                return null;
            return data.Select(x => _transform(x)).ToList();
        }
    }

    public class DataStreamWithProvider<T>: IDataStream<T>
    {
        private readonly Queue<T> _items = new Queue<T>();
        private bool _isCompleted = false;

        private int _requestCount = -1;
        private TaskCompletionSource<bool>? _requestCompleted = null;

        public void AddItem(T item)
        {
            lock (_items)
            {
                _items.Enqueue(item);
                CheckForRequestCompletion();
            }
        }
        public void AddItems(IEnumerable<T> items)
        {
            lock (_items)
            {
                foreach (var item in items)
                    _items.Enqueue(item);
                CheckForRequestCompletion();
            }
        }

        public void NotifyComplete()
        {
            _isCompleted = true;
            lock (_items)
                CheckForRequestCompletion();
        }

        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            if (count == 0)
                throw new ArgumentException("count is zero");

            lock (_items)
            {
                if (!_isCompleted || _items.Count < count)
                {
                    _requestCompleted = new TaskCompletionSource<bool>();
                    _requestCount = count;
                } else
                {
                    _requestCompleted = null;
                }
            }

            if (_requestCompleted != null)
                await _requestCompleted.Task.ConfigureAwait();

            lock (_items)
            {
                var l = new List<T>();

                count = Math.Min(count, _items.Count);

                for (int i = 0; i < count; i++)
                    l.Add(_items.Dequeue());

                return l;
            }
        }

        private void CheckForRequestCompletion()
        {
            if (_requestCount > 0 && (_requestCount >= _items.Count || _isCompleted))
            {
                _requestCompleted!.SetResult(true);
                _requestCount = -1;
            }
        }

    }

    public class MultipleSourcesDataStream<T> : IDataStream<T>
    {
        private readonly Queue<IDataStream<T>> _sources;
        private IDataStream<T> _current;

        private readonly List<T> _data = new List<T>();

        public MultipleSourcesDataStream(IEnumerable<IDataStream<T>> sources)
        {
            _sources = new Queue<IDataStream<T>>(sources);
            _current = _sources.Dequeue();
        }

        public async Task<List<T>?> ReadMoreAsync(int count)
        {
            _data.Clear();
            if (_current == null)
                return _data;

            while (_data.Count < count)
            {
                var needToRead = count - _data.Count;
                var result = await _current.ReadMoreAsync(needToRead).ConfigureAwait();
                if (result == null)
                    return null;

                _data.AddRange(result);
                if (result.Count == 0)
                {
                    if (!_sources.TryDequeue(out _current))
                        break;
                }
            }
            return _data;
        }
    }

    public class BasicCachedDataStream<T> : IDataStreamProvider<T>
    {
        private readonly List<T> _cache = new List<T>();
        private readonly IDataStream<T>? _dataStream = null;
        private bool _readAll = false;
        private Task<List<T>?>? _currentReadOp;

        public BasicCachedDataStream(IDataStream<T> dataStream, List<T>? data = null)
        {
            _dataStream = dataStream;
            if (data != null)
                _cache.AddRange(data);
        }

        public BasicCachedDataStream(List<T> data)
        {
            _cache = data;
            _readAll = true;
        }

        public IDataStream<T> GetStream() => new Reader(this);

        private async Task<List<T>?> GetRangeThreaded(int index, int count)
        {
            if (_readAll || index + count <= _cache.Count)
                return GetFromCache(index, count);

            if (_currentReadOp != null && !_currentReadOp.IsCompleted)
                await _currentReadOp.ConfigureAwait();

            _currentReadOp = GetRange(index, count);

            var result = await _currentReadOp.ConfigureAwait();
            _currentReadOp = null;

            return result;
        }

        private async Task<List<T>?> GetRange(int index, int count)
        {
            var fromCache = GetFromCache(index, count);
            if (_readAll || count == fromCache.Count)
                return fromCache;

            index += fromCache.Count;
            var res = await _dataStream!.ReadMoreAsync(count - fromCache.Count).ConfigureAwait();
            if (res == null)
                return null;

            if (res.Count == 0)
                _readAll = true;

            _cache.AddRange(res);

            return GetFromCache(index, count);
        }

        private List<T> GetFromCache(int index, int count)
        {
            int inCacheCount = Math.Min(count, _cache.Count - index);
            return inCacheCount > 0 ? _cache.GetRange(index, inCacheCount) : new List<T>();
        }

        public class Reader : IDataStream<T>
        {
            private int _index = 0;
            private readonly BasicCachedDataStream<T> _stream;

            public Reader(BasicCachedDataStream<T> stream)
            {
                _stream = stream;
            }

            public async Task<List<T>?> ReadMoreAsync(int count)
            {
                var res = await _stream.GetRangeThreaded(_index, count).ConfigureAwait();
                if (res != null)
                    _index += res.Count;
                return res;
            }
        }
    }
}

using SmartCacheLibrary.Interfaces;
using SmartCacheLibrary.Model;

namespace SmartCacheLibrary
{
    public class SmartCacheService<TKey, TValue> : ISmartCacheService<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Dictionary<TKey, LinkedListNode<SmartCacheItem<TKey, TValue>>> _cacheData;
        private readonly LinkedList<SmartCacheItem<TKey, TValue>> _lruList;

        public SmartCacheService(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            _capacity = capacity;
            _cacheData = new Dictionary<TKey, LinkedListNode<SmartCacheItem<TKey, TValue>>>(capacity);
            _lruList = new LinkedList<SmartCacheItem<TKey, TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_cacheData.TryGetValue(key, out var existingNode))
                {
                    existingNode.Value.Value = value;
                    existingNode.Value.IncrementAccess();
                    MoveToHead(existingNode);
                    return;
                }

                if (_cacheData.Count >= _capacity)
                {
                    RemoveLeastUsed();
                }

                var newItem = new SmartCacheItem<TKey, TValue>(key, value);
                var newNode = new LinkedListNode<SmartCacheItem<TKey, TValue>>(newItem);
                _lruList.AddFirst(newNode);
                _cacheData[key] = newNode;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();

            try
            {
                _lruList.Clear();
                _cacheData.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();

            try
            {
                return _cacheData.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue Get(TKey key)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cacheData.TryGetValue(key, out var item))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        item.Value.IncrementAccess();
                        MoveToHead(item);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }

                    return item.Value.Value;
                }

                throw new KeyNotFoundException($"Key '{key}' not found in cache.");
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetMostFrequentlyAccessed(int count)
        {
            _lock.EnterReadLock();
            try
            {
                return _lruList
                    .OrderByDescending(n => n.AccessCount)
                    .Take(count)
                    .Select(n => new KeyValuePair<TKey, TValue>(n.Key, n.Value))
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            _lock.EnterWriteLock();

            try
            {
                if (_cacheData.TryGetValue(key, out var item))
                {
                    _lruList.Remove(item);
                    _cacheData.Remove(key);

                    return true;
                }

                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void MoveToHead(LinkedListNode<SmartCacheItem<TKey, TValue>> node)
        {
            if (node.List != _lruList)
            {
                return;
            }

            _lruList.Remove(node);
            _lruList.AddFirst(node);
        }

        private void RemoveLeastUsed()
        {
            var lastNode = _lruList.Last;
            if (lastNode == null)
            {
                return;
            }

            _lruList.RemoveLast();
            _cacheData.Remove(lastNode.Value.Key);
        }
    }
}

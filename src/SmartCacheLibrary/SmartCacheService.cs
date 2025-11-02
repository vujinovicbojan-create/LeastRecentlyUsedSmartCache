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
                var newNode = _lruList.AddFirst(newItem);
                _cacheData[key] = newNode;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public TValue Get(TKey key)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_cacheData.TryGetValue(key, out var node))
                {
                    throw new KeyNotFoundException($"Key '{key}' not found in cache.");
                }

                _lock.EnterWriteLock();
                try
                {
                    node.Value.IncrementAccess();
                    MoveToHead(node);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                return node.Value.Value;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_cacheData.TryGetValue(key, out var node))
                {
                    _lruList.Remove(node);
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

        public IEnumerable<KeyValuePair<TKey, TValue>> GetMostFrequentlyAccessed(int count)
        {
            _lock.EnterReadLock();
            try
            {
                return _lruList
                    .OrderByDescending(item => item.AccessCount)
                    .Take(count)
                    .Select(item => new KeyValuePair<TKey, TValue>(item.Key, item.Value))
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private void MoveToHead(LinkedListNode<SmartCacheItem<TKey, TValue>> node)
        {
            if (_lruList.First == node) 
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
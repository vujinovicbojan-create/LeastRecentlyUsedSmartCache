namespace SmartCacheLibrary.Interfaces
{
    public interface ISmartCacheService<TKey, TValue>
    {
        public void Add(TKey key, TValue value);
        public TValue Get(TKey key);
        public bool Remove(TKey key);
        public bool ContainsKey(TKey key);
        public void Clear();
        public IEnumerable<KeyValuePair<TKey, TValue>> GetMostFrequentlyAccessed(int count);
    }
}

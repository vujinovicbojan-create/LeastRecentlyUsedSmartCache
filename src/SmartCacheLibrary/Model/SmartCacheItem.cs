namespace SmartCacheLibrary.Model
{
    public class SmartCacheItem<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
        public int AccessCount { get; set; }
        public SmartCacheItem(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            AccessCount = 1;
        }

        public void IncrementAccess() { AccessCount++; }
    }
}

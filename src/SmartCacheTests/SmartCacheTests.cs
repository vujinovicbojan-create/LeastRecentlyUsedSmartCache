using SmartCacheLibrary.Interfaces;

namespace SmartCacheTests;

public class SmartCacheTests : IClassFixture<SmartCacheFixture>
{
    private readonly ISmartCacheService<int, string> _cacheService;

    public SmartCacheTests(SmartCacheFixture fixture)
    {
        _cacheService = fixture.CacheService;
    }

    [Fact]
    public void LRU_Eviction_Works()
    {
        _cacheService.Add(1, "Apple");
        _cacheService.Add(2, "Samsung");
        _cacheService.Add(3, "Xiaomi");

        // Access some items
        _cacheService.Get(2);

        // Add 4th item, should evict least recently used (1)
        _cacheService.Add(4, "Nokia");

        Assert.False(_cacheService.ContainsKey(1)); // 1 should be evicted
        Assert.True(_cacheService.ContainsKey(2));
        Assert.True(_cacheService.ContainsKey(3));
        Assert.True(_cacheService.ContainsKey(4));
    }

    [Fact]
    public void Frequency_Tracking_Works()
    {
        _cacheService.Add(1, "Apple");
        _cacheService.Add(2, "Samsung");
        _cacheService.Add(3, "Xiaomi");

        // Access keys multiple times
        _cacheService.Get(1);
        _cacheService.Get(1);
        _cacheService.Get(2);

        var top2 = _cacheService.GetMostFrequentlyAccessed(2).ToList();

        Assert.Equal(1, top2[0].Key); // Key 1 most frequently
        Assert.Equal(2, top2[1].Key); // Key 2 second
    }

    [Fact]
    public void Thread_Safety_Works()
    {
        
    }
}

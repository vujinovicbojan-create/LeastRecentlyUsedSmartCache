# LeastRecentlyUsedSmartCache
Implementation for generic SmartCache

Requirements:

Implement a generic `SmartCache<TKey, TValue>` class with the following requirements:
- Store key-value pairs with a maximum capacity (set via constructor).

- When capacity is exceeded, remove the **least recently accessed** item (LRU eviction).

- Track access frequency for each item.

- Provide a method `GetMostFrequentlyAccessed(int count)` that returns the top N most frequently accessed items.

- The cache must be thread-safe.

- Include methods: `Add`, `Get`, `Remove`, `Clear`, `ContainsKey`.

**Additional Requirements:**

- Both `Get` and `Add` operations should count as "access" for frequency tracking.

- Explain your choice of internal data structures and how you achieved O(1) or near O(1) operations.

**Unit Tests:**

- Test LRU eviction behavior

- Test frequency tracking

- Test thread safety with concurrent access

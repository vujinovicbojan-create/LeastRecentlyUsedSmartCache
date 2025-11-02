using Microsoft.Extensions.DependencyInjection;
using SmartCacheLibrary;
using SmartCacheLibrary.Interfaces;

var services = new ServiceCollection();

services.AddSingleton(typeof(ISmartCacheService<,>), typeof(SmartCacheService<,>));

var provider = services.BuildServiceProvider();

// create instance with 3 params
var smartCache = ActivatorUtilities.CreateInstance<SmartCacheService<int, string>>(provider, 3);

smartCache.Add(1, "Apple");
smartCache.Add(2, "Samsung");
smartCache.Add(3, "Xaomi");

Console.WriteLine($"Get key 2: {smartCache.Get(2)}"); // get Samsung

// Add one more element
smartCache.Add(4, "Nokia");

Console.WriteLine("Cache contains key 1? " + smartCache.ContainsKey(1)); // False
Console.WriteLine("Cache contains key 4? " + smartCache.ContainsKey(4)); // True

var top = smartCache.GetMostFrequentlyAccessed(2);
Console.WriteLine("Top accessed:");
foreach (var item in top)
{
    Console.WriteLine($"{item.Key} => {item.Value}");
}

smartCache.Remove(2);
smartCache.Clear();

Console.WriteLine("Finished.");




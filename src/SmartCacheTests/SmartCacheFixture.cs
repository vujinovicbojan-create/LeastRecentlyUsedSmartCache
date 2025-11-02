using Microsoft.Extensions.DependencyInjection;
using SmartCacheLibrary;
using SmartCacheLibrary.Interfaces;

namespace SmartCacheTests
{
    public class SmartCacheFixture
    {
        public ISmartCacheService<int, string> CacheService { get; }

        public SmartCacheFixture()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ISmartCacheService<,>), typeof(SmartCacheService<,>));
            var provider = services.BuildServiceProvider();

            CacheService = ActivatorUtilities.CreateInstance<SmartCacheService<int, string>>(provider, 3);
        }
    }
}

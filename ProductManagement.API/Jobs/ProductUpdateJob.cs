using Microsoft.Extensions.Caching.Distributed;
using ProductManagement.Core.Interfaces;
using System.Text.Json;

namespace ProductManagement.API.Jobs
{
    public class ProductUpdateJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public ProductUpdateJob(IUnitOfWork unitOfWork, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task RefreshProductsCache()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync("product_list", JsonSerializer.Serialize(products), cacheOptions);

            Console.WriteLine($"[JOB] Products cache updated at {DateTime.Now}");
        }
    }
}

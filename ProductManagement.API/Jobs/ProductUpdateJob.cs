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
            // 1️⃣ جلب كل المنتجات من SQL
            var products = await _unitOfWork.Products.GetAllAsync();

            // 2️⃣ تحويل المنتجات لـ JSON
            var jsonData = JsonSerializer.Serialize(products);

            // 3️⃣ تخزينهم في Redis
            await _cache.SetStringAsync("products_cache", jsonData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // الكاش صالح لمدة ساعة
            }); 

            Console.WriteLine($"[JOB] Products cache updated at {DateTime.Now}");
        }
    }
}

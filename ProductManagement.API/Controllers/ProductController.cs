using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductManagement.Core.Interfaces;
using ProductManagement.Core.Models;
using ProductManagement.EF.Data;
using System.Text.Json;

namespace ProductManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public ProductController(IUnitOfWork unitOfWork, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string cacheKey = "product_list";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var productsFromCache = JsonSerializer.Deserialize<IEnumerable<Product>>(cachedData);
                return Ok(productsFromCache);
            }

            var products = await _unitOfWork.Products.GetAllAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // يخزن البيانات لمدة 5 دقائق
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), cacheOptions);

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string cacheKey = $"product_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var productFromCache = JsonSerializer.Deserialize<Product>(cachedData);
                return Ok(productFromCache);
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), cacheOptions);

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            // Clear cache for product list
            await _cache.RemoveAsync("product_list");
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product updatedProduct)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.ManualPdfPath = updatedProduct.ManualPdfPath;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();
            // Remove product from cache
            await _cache.RemoveAsync($"product_{id}");
            await _cache.RemoveAsync("product_list");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();
            // Remove product from cache
            await _cache.RemoveAsync($"product_{id}");
            await _cache.RemoveAsync("product_list");
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace ProductManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisTestController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public RedisTestController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet("set")]
        public async Task<IActionResult> SetCache()
        {
            var data = new { Name = "Eman", Role = "Developer" };
            var jsonData = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(jsonData);

            await _cache.SetAsync("user:1", bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return Ok("Data cached in Redis.");
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCache()
        {
            var bytes = await _cache.GetAsync("user:1");
            if (bytes == null)
                return NotFound("No data found in cache.");

            var jsonData = Encoding.UTF8.GetString(bytes);
            var data = JsonSerializer.Deserialize<object>(jsonData);

            return Ok(data);
        }
    }
}
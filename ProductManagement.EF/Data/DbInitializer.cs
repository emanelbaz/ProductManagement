using ProductManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.EF.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            // لو فيه بيانات بالفعل، مفيش داعي نضيف تاني
            if (context.Products.Any())
                return;

            var products = new List<Product>
            {
                new Product { Name = "Laptop", Description = "Gaming Laptop 16GB RAM", Price = 1500 },
                new Product { Name = "Mouse", Description = "Wireless Bluetooth Mouse", Price = 25 },
                new Product { Name = "Keyboard", Description = "Mechanical RGB Keyboard", Price = 120 },
                new Product { Name = "Monitor", Description = "27-inch 144Hz Monitor", Price = 300 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}

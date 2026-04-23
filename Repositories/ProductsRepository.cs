using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Text.Json;
namespace Repositories


{

    public class ProductsRepository : IProductsRepository
    {

        public readonly ApiShopContext _context;
        public ProductsRepository(ApiShopContext context)
        {
            _context = context;
        }
        public async Task<(List<Product> Items, int TotalCount)> GetProducts(int position, int skip, int?[] categoryIds,
            string? description, int? maxPrice, int? minPrice)
        {
            var query = _context.Products.Where(product =>
                (description == null ? (true) : (product.Description.Contains(description))) &&
                ((maxPrice == null) ? (true) : (product.Price <= maxPrice)) &&
                ((minPrice == null) ? (true) : (product.Price >= minPrice)) &&
                ((categoryIds.Length == 0) ? (true) : (categoryIds.Contains(product.CategoryId)))).OrderBy(product => product.Price);
            List<Product> products = await query.Skip((position - 1) * skip).Take(skip).Include(product => product.Category).ToListAsync();
            var total = await query.CountAsync();
            return (products, total);
        }
        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _context.Products.Include(product => product.Category).ToListAsync();
        }

        public async Task<Product?> GetProduct(int id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProduct(int id, Product product)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return null;
            existing.ProductName = product.ProductName;
            existing.Price = product.Price;
            existing.CategoryId = product.CategoryId;
            existing.Description = product.Description;
            existing.ImageUrl = product.ImageUrl;
            existing.IsAvailable = product.IsAvailable;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return false;
            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
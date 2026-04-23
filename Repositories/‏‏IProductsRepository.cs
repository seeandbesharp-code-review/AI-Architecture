using Entities;

namespace Repositories
{
    public interface IProductsRepository
    {
        public Task<(List<Product> Items, int TotalCount)> GetProducts(int position, int skip, int?[] categoryIds,
         string? description, int? maxPrice, int? minPrice);
        public Task<IEnumerable<Product>> GetProducts();
        public Task<Product?> GetProduct(int id);
        public Task<Product> AddProduct(Product product);
        public Task<Product?> UpdateProduct(int id, Product product);
        public Task<bool> DeleteProduct(int id);
    }
}
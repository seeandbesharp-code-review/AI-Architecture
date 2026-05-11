using Entities;
using DTOs;
namespace Services
{
    public interface IProductsServices
    {
        public Task<PageResponseDTO<ProductDTO>> GetProducts(int position, int skip, int?[] categoryIds,
          string? description, int? maxPrice, int? minPrice);
        public Task<IEnumerable<ProductDTO>> GetProducts();
        public Task<ProductDTO?> GetProduct(int id);
        public Task<ProductDTO> AddProduct(ProductDTO product);
        public Task<ProductDTO?> UpdateProduct(int id, ProductDTO product);
        public Task<bool> DeleteProduct(int id);
        public Task<IEnumerable<ProductDTO>> GetAvailableProducts();
    }
}
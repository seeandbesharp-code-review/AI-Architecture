using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsServices _IProductsServices;
        public ProductsController(IProductsServices productsServices)
        {
            _IProductsServices = productsServices;
        }
       
        [HttpGet]
        public async Task<ActionResult<PageResponseDTO<ProductDTO>>> Get(int position, int skip, [FromQuery] int?[] categoryIds, string? description, int? maxPrice, int? minPrice)
        {

            PageResponseDTO<ProductDTO> pageResponse = await _IProductsServices.GetProducts(position, skip, categoryIds, description, maxPrice, minPrice);
            if (pageResponse.Data != null && pageResponse.Data.Count > 0)
                return Ok(pageResponse);
            return NoContent();
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            var product = await _IProductsServices.GetProduct(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductDTO product)
        {
            var created = await _IProductsServices.AddProduct(product);
            return CreatedAtAction(nameof(Get), new { }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> Put(int id, [FromBody] ProductDTO product)
        {
            var updated = await _IProductsServices.UpdateProduct(id, product);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _IProductsServices.DeleteProduct(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAvailable()
        {
            var products = await _IProductsServices.GetAvailableProducts();
            if (!products.Any()) return NoContent();
            return Ok(products);
        }

    }
}



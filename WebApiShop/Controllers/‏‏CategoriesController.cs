using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesServices _ICategoriesServices;
        public CategoriesController(ICategoriesServices categoriesServices)
        {
            _ICategoriesServices = categoriesServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> Get()
        {
            IEnumerable<CategoryDTO> categories = await _ICategoriesServices.GetCategories();
            if (categories != null && categories.Any())
                return Ok(categories);
            return NoContent();
        }
    }
}


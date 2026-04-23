using Microsoft.AspNetCore.Mvc;
using Services;
using AutoMapper;
using DTOs;


namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersServices _ordersServices;
        public OrdersController(IOrdersServices ordersServices, IMapper mapper)
        {
            _ordersServices = ordersServices;
        }
       
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> Get(int id)
        {
            OrderDTO? order = await _ordersServices.GetOrderById(id);
            if (order != null)
                return Ok(order);
            return NotFound();
        }

        [HttpPost]
       public async Task<ActionResult<OrderDTO>> Post([FromBody] OrderDTO newOrder)
        {
            OrderDTO? placedOrder = await _ordersServices.AddOrder(newOrder);
            if (placedOrder == null)
                return BadRequest("Invalid order. Please check order items and products.");
            return CreatedAtAction(nameof(Get), new { id = placedOrder.OrderId }, placedOrder);
        }






    }
}



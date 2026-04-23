using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories


{

    public class OrdersRepository : IOrdersRepository
    {
       public readonly ApiShopContext _context;
        public OrdersRepository(ApiShopContext context)
        {
            _context = context;
        }
        public async Task<Order?> GetOrderById(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }
        public async Task<Order> AddOrder(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }


    }
}

using EcommerceApp.Data;
using EcommerceApp.Models.Database;
using EcommerceApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly EcommerceContext _context;
        public OrdersRepository(EcommerceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Order>> GetOrders(CancellationToken cancellation)
        {
            return await _context.Orders.ToListAsync(cancellation);
        }
        public void Add(Order order)
        {
            _context.Orders.Add(order);
        }
        public async Task<List<Order>> GetOrderByUserId(string userId, CancellationToken cancellation)
        {
            return await _context.Orders
                .Where(order => order.UserUid == userId)
                .ToListAsync(cancellation);
        }
        public async Task<Order> GetOrder(string userId, string orderUid, CancellationToken cancellation)
        {
            return await _context.Orders
                .Include(o => o.Products)
                .Where(order => order.Id == orderUid && order.UserUid == userId)
                .FirstOrDefaultAsync(cancellation);
        }
        public async Task<Order> GetOrder(string orderId, CancellationToken cancellation)
        {
            return await _context.Orders.Where(order => order.Id == orderId).FirstOrDefaultAsync(cancellation);
        }
    }
}

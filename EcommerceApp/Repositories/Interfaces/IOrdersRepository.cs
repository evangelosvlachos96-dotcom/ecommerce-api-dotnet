using EcommerceApp.Models.Database;

namespace EcommerceApp.Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        Task<List<Order>> GetOrders(CancellationToken cancellation);
        Task<Order> GetOrder(string orderUid, CancellationToken cancellation);
        Task<Order> GetOrder(string userId, string orderUid, CancellationToken cancellation);
        Task<List<Order>> GetOrderByUserId(string userId, CancellationToken cancellation); 
        void Add(Order order);
    }
}

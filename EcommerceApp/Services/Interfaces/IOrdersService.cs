using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Models.Requests;

namespace EcommerceApp.Services.Interfaces
{
    public interface IOrdersService
    {
        Task<InternalDataTransfer<List<Order>>> GetOrders();
        Task<InternalDataTransfer<bool>> ConfirmOrder(string userId, string identifier);
        Task<InternalDataTransfer<byte[]>> GetInvoice(string userId, string identifier);
        Task<InternalDataTransfer<Order>> CreateOrder(CreateOrderRequest request);
        Task<InternalDataTransfer<List<Order>>> GetOrderByUserId(string userId);
        Task<InternalDataTransfer<string>> Checkout(string userId, string identifier);
        Task<InternalDataTransfer<bool>> Dispatch(string userId, string identifier);
        Task<InternalDataTransfer<bool>> SendInvoice(string userId, string identifier);
        

    }
}

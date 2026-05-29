using EcommerceApp.Models.Database;

namespace EcommerceApp.Services.Interfaces
{
    public interface IPaymentService
    {
        string CreateSession(List<Product> productList, string orderId, string email);
    }
}

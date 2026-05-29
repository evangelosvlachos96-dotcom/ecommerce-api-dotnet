using EcommerceApp.Models.Database;

namespace EcommerceApp.Services.Interfaces
{
    public interface IInvoiceGeneratorService
    {
        string GenerateInvoice(Order order);
    }
}

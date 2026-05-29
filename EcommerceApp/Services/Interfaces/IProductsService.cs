using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Models.Requests;

namespace EcommerceApp.Services.Interfaces
{
    public interface IProductsService
    {
        Task<InternalDataTransfer<List<Product>>> GetProducts();
        Task<InternalDataTransfer<Product>> UpdateProduct(UpdateProductRequest request);
        Task<InternalDataTransfer<bool>> DeleteProduct(int identifier);
        Task<InternalDataTransfer<List<Product>>> SyncProductsOnDemand();
    }
}
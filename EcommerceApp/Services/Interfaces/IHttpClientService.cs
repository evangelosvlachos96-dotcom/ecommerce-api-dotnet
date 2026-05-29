using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;

namespace EcommerceApp.Services.Interfaces
{
    public interface IHttpClientService
    {
        Task<InternalDataTransfer<List<Product>>> GetRequest(string url);
    }
}

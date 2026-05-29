using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Services.Interfaces;
using System.Text.Json;

namespace EcommerceApp.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<InternalDataTransfer<List<Product>>> GetRequest(string url)
        {
            var client = _httpClientFactory.CreateClient();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                List<Product> products = await client.GetFromJsonAsync<List<Product>>(url, options);

                return new InternalDataTransfer<List<Product>>(products);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }   
}

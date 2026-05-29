using EcommerceApp.Models.Database;

namespace EcommerceApp.Repositories.Interfaces
{
    public interface IProductsRepository
    {
        Task<List<Product>> GetProducts(CancellationToken cancellation); 
        Task<Product> GetProduct(int identifier); 
        void Delete(Product product);
        Task<List<Product>> GetProductsById(List<int> productList);
    }
}

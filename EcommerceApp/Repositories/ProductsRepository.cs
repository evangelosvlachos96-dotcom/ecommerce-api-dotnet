using EcommerceApp.Data;
using EcommerceApp.Models.Database;
using EcommerceApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly EcommerceContext _context;
        public ProductsRepository(EcommerceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Product>> GetProducts(CancellationToken cancellation)
        {
            return await _context.Products.ToListAsync(cancellation);
        }
        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }
        public async Task<Product> GetProduct(int identifier)
        {
            return await _context.Products.FirstOrDefaultAsync(product => product.Id == identifier);
        }
        public async Task<List<Product>> GetProductsById(List<int> productList)
        {
            return await _context.Products
                .Where(p => productList.Contains(p.Id)).ToListAsync();
        }
    }
}

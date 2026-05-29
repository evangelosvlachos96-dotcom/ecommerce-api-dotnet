using EcommerceApp.Data;
using EcommerceApp.Constants;
using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Models.Requests;
using EcommerceApp.Repositories.Interfaces;
using EcommerceApp.Services.Interfaces;
using EcommerceApp.Utils;
using Quartz;

namespace EcommerceApp.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly EcommerceContext _context;
        private readonly ISchedulerFactory _schedulerFactory;

        public ProductsService(IProductsRepository productsRepository, EcommerceContext context, ISchedulerFactory schedulerFactory)
        {
            _productsRepository = productsRepository;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _schedulerFactory = schedulerFactory;
        }

        public async Task<InternalDataTransfer<List<Product>>> GetProducts()
        {
            List<Product> products = await _productsRepository.GetProducts(default);

            return new InternalDataTransfer<List<Product>>(products);
        }

        public async Task<InternalDataTransfer<Product>> UpdateProduct(UpdateProductRequest request)
        {
            Product product = await _productsRepository.GetProduct(request.Id);

            if (product == null)
            {
                return new InternalDataTransfer<Product>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            // Update the properties of the existing product only if the fields are not null or empty
            CheckAndMap(product, request);

            await _context.SaveChangesAsync();

            return new InternalDataTransfer<Product>(product);
        }

        public async Task<InternalDataTransfer<bool>> DeleteProduct(int identifier)
        {
            Product product = await _productsRepository.GetProduct(identifier);

            if (product == null)
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            _productsRepository.Delete(product);
            await _context.SaveChangesAsync();

            return new InternalDataTransfer<bool>(true);
        }

        public async Task<InternalDataTransfer<List<Product>>> SyncProductsOnDemand()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey("SyncProductsJob"); 

            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.TriggerJob(jobKey);


                var res = await GetProducts();

                if (res.Status)
                {
                    return new InternalDataTransfer<List<Product>>(res.Data);
                }
            }

            return new InternalDataTransfer<List<Product>>(false, ProjectErrorCodes.GenericError.GetName());
        }

        private void CheckAndMap(Product existingProduct, UpdateProductRequest updateRequest)
        {
            if (!string.IsNullOrWhiteSpace(updateRequest.Title))
            {
                existingProduct.Title = updateRequest.Title;
            }

            if (updateRequest.Price > 0)
            {
                existingProduct.Price = updateRequest.Price;
            }

            if (!string.IsNullOrWhiteSpace(updateRequest.Description))
            {
                existingProduct.Description = updateRequest.Description;
            }

            if (updateRequest.Rating != null)
            {
                // Assuming we only want to update the rate and count if they are valid
                if (updateRequest.Rating.Rate >= 0)
                {
                    existingProduct.Rating.Rate = updateRequest.Rating.Rate;
                }

                if (updateRequest.Rating.Count >= 0)
                {
                    existingProduct.Rating.Count = updateRequest.Rating.Count;
                }
            }
        }
    }
}

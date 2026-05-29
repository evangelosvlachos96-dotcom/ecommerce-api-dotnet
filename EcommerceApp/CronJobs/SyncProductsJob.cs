using EcommerceApp.Data;
using EcommerceApp.Repositories.Interfaces;
using EcommerceApp.Services.Interfaces;
using Quartz;

namespace EcommerceApp.CronJobs
{
    public class SyncProductsJob : IJob
    {
        private readonly IHttpClientService _client;
        private readonly EcommerceContext _context;

        public SyncProductsJob(EcommerceContext context, IHttpClientService client)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await SyncAll();
        }

        public async Task SyncAll(string url = "https://fakestoreapi.com/products/")
        {
            var result = await _client.GetRequest(url);

            if (result.Status)
            {
                try
                {
                    // Replace the catalog atomically: drop the existing rows and insert the fresh set.
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    _context.Products.RemoveRange(_context.Products);
                    await _context.Products.AddRangeAsync(result.Data);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}

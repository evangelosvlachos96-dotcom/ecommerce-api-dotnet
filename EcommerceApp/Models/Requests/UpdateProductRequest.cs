using EcommerceApp.Models.Database;

namespace EcommerceApp.Models.Requests
{
    public class UpdateProductRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public RateValueObject Rating { get; set; }
    }
}

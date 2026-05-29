namespace EcommerceApp.Models.Database
{
    public class Product
    {
        public Product()
        {
        }

        public Product(int id, string title, decimal price, string description, string category, string imagePath, double ratingRate, int ratingCount)
        {
            Id = id;
            Title = title;
            Price = price;
            Description = description;
            Category = category;
            Image = imagePath;
            Rating = new RateValueObject()
            {
                Count = ratingCount,
                Rate = ratingRate
            };
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public RateValueObject Rating { get; set; } 
        // Navigation property for the many-to-many relationship with Order
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
    public class RateValueObject
    {
        public double Rate { get; set; }
        public int Count { get; set; }
    }

}

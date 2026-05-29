using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApp.Models.Database
{
    public class Order
    {
        private Order()
        {

        }
        public Order(List<int> products, decimal totalPrice, string userId)
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            ProductUids = products;
            TotalPrice = totalPrice;
            UserUid = userId; 
            
            Products = new List<Product>();
        }
        public string Id { get; private set; }
        public string UserUid { get; private set; }

        public decimal TotalPrice { get; private set; }
        public OrderStatus Status { get; private set; }
        public string Invoice { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        [NotMapped]
        public List<int> ProductUids { get; private set; }

        public enum OrderStatus
        {
            Submitted,
            Paid,
            Dispatched
        }

        public void MarkPaid()
        {
            if (Status == OrderStatus.Submitted)
            {
                Status = OrderStatus.Paid;
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public void MarkDispatched()
        {
            if (Status == OrderStatus.Paid && !string.IsNullOrEmpty(Invoice))
            {
                Status = OrderStatus.Dispatched;
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public void StoreInvoice(string invoiceBase64)
        {
            if (Status == OrderStatus.Paid && string.IsNullOrEmpty(Invoice))
            {
                Invoice = invoiceBase64;
                UpdatedAt = DateTime.UtcNow;
            }
        }

    }
}

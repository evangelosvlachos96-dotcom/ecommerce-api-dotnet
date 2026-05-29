using System.Text.Json.Serialization;

namespace EcommerceApp.Models.Requests
{
    public class CreateOrderRequest
    {
        [JsonIgnore]
        public string? UserId { get; set; }
        public List<int> Products {get;set;} 
    }
}

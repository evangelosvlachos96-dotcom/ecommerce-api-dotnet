using System.Security.Claims;

namespace EcommerceApp.Models.DTO
{
    public class TokenReq
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public Dictionary<string, bool> Claims { get; set; }
    }
}

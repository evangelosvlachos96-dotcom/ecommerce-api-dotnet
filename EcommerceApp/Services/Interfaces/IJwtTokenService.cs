using EcommerceApp.Models.DTO;
using EcommerceApp.Models.DTO.Base;

namespace EcommerceApp.Services.Interfaces
{
    public interface IJwtTokenService
    {
        InternalDataTransfer<string> GenerateToken(TokenReq request);
    }
}

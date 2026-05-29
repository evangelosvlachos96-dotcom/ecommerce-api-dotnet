using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Models.Requests;
using EcommerceApp.Models.Responses;

namespace EcommerceApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<InternalDataTransfer<string>> Register(UserRegisterRequest request);
        Task<InternalDataTransfer<UserLoginResponse>> Login(UserLoginRequest request);
    }
}

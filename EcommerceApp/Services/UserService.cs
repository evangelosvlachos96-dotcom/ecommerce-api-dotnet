using EcommerceApp.Data;
using EcommerceApp.Constants;
using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO;
using EcommerceApp.Repositories.Interfaces;
using EcommerceApp.Services.Interfaces;
using EcommerceApp.Models.Requests;
using EcommerceApp.Models.Responses;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Utils;

namespace EcommerceApp.Services
{
    public class UserService : IUserService
    {
        private readonly EcommerceContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _tokenService;
        public UserService(EcommerceContext context, IUserRepository userRepository, IJwtTokenService tokenService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<InternalDataTransfer<UserLoginResponse>> Login(UserLoginRequest request)
        {
            var hashedTryEmail = EncryptionHelper.Encrypt(request.Email);
            var user = await _userRepository.GetByEmail(hashedTryEmail);
            if (user == null)
                return new InternalDataTransfer<UserLoginResponse>(false, ProjectErrorCodes.NotExisting.GetName());

            var hashedTryPass = EncryptionHelper.Encrypt(request.Password);
            if (hashedTryEmail != user.Email || hashedTryPass != user.Password)
            {
                return new InternalDataTransfer<UserLoginResponse>(false, ProjectErrorCodes.InvalidInternalVerification.GetName());
            }

            var token = _tokenService.GenerateToken(new TokenReq
            {
                Email = request.Email,
                UserId = user.Id,
                Claims = new Dictionary<string, bool>
                {
                    {"admin", user.Role == AppUserRoles.Admin }
                }
            });

            return new InternalDataTransfer<UserLoginResponse>(new UserLoginResponse
            {
                Token = token.Data
            });
        }

        public async Task<InternalDataTransfer<string>> Register(UserRegisterRequest request)
        {
            if (request == null)
                return new InternalDataTransfer<string>(false, ProjectErrorCodes.InvalidPayload.GetName());
            var hashedEmail = EncryptionHelper.Encrypt(request.Email);

            var checkExistingUser = await _userRepository.GetByEmail(hashedEmail);
            if (checkExistingUser != null)
                return new InternalDataTransfer<string>(false, ProjectErrorCodes.EmailExists.GetName());

            var hashedPass = EncryptionHelper.Encrypt(request.Password);
            User newUser = new User(request.FirstName, request.LastName, hashedEmail, hashedPass);

            _userRepository.Add(newUser);
            await _context.SaveChangesAsync();

            return new InternalDataTransfer<string>("Ok");
        }
    }
}

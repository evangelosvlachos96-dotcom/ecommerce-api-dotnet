using EcommerceApp.Models.DTO;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApp.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private const string TokenSecret = "TokenThatSupposeToBeInASagerPlace";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);

        public InternalDataTransfer<string> GenerateToken(TokenReq request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, request.Email),
                new(JwtRegisteredClaimNames.Email, request.Email),
                new("userid", request.UserId)
            };

            foreach (var claimPair in request.Claims)
            {
                var valueType = claimPair.Value switch
                {
                    true => ClaimValueTypes.Boolean,
                    false => ClaimValueTypes.Boolean
                };

                var claim = new Claim(claimPair.Key, claimPair.Value.ToString()!, valueType);
                claims.Add(claim);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = "Issuer",
                Audience = "Audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return new InternalDataTransfer<string>(jwt);
        }
    }
}

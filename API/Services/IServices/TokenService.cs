using API.Models;
using API.Utility;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services.IServices
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration config;
        private readonly SymmetricSecurityKey jwtKey;

        public TokenService(IConfiguration config)
        {
            this.config = config;
            this.jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
        }
        public string CreateJWT(AppUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(SD.UserId, user.Id.ToString()),
                new Claim(SD.UserName , user.UserName),
                new Claim(SD.Email, user.Email),
            };

            var creds = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:ExpiresInDays"])),
                Issuer = config["JWT:Issuer"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(jwt);
        }
    }
}

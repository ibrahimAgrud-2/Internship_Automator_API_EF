using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EF_InternApplicationAutomator.API.Service
{
    public class TokenService
    {

        public const string UserIdClaim = "UserID";
        public const string InternIdClaim = "InternID";

        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

  
        public string CreateToken(string personID,string role, string? userID,string? internID)
        {
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, personID),
                        new Claim(ClaimTypes.Role, role),
                    };

            if (role == "User")
            {
                claims.Add(new Claim(UserIdClaim, userID!));
            }
            else if (role == "Intern")
            {
                claims.Add(new Claim(InternIdClaim, internID!));
            }
            else
            {
                throw new InvalidOperationException($"Bilinmeyen rol: {role}");
            }
            string secret = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ayarı bulunamadı (Login-secrets).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

     
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"]!)),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

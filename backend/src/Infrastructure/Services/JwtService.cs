using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductCatalogAPI.Infrastructure.Services;

namespace ProductCatalogAPI.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;

    public JwtService()
    {
        _secretKey = "&Az3mUA+,j!z5qY+:tBVid_;1&PdTRHq";
    }

    public string GenerateToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "api-user"),
                new Claim(ClaimTypes.Role, "user")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
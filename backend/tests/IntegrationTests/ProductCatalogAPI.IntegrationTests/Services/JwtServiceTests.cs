namespace ProductCatalogAPI.IntegrationTests.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductCatalogAPI.Infrastructure.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly string _expectedSecretKey = "&Az3mUA+,j!z5qY+:tBVid_;1&PdTRHq";

    public JwtServiceTests()
    {
        _jwtService = new JwtService();
    }
    
    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        // Act
        var token = _jwtService.GenerateToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.IsType<string>(token);
    }

    [Fact]
    public void GenerateToken_ReturnsTokenWithCorrectStructure()
    {
        // Act
        var token = _jwtService.GenerateToken();

        // Assert - Un JWT válido tiene 3 partes separadas por puntos
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.All(parts, part => Assert.NotEmpty(part));
    }
    
    [Fact]
    public void GenerateToken_HasCorrectExpiration()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
        var expectedMinExpiration = DateTime.UtcNow.AddHours(1).AddMinutes(-1); // -1 minuto de tolerancia
        var expectedMaxExpiration = DateTime.UtcNow.AddHours(1).AddMinutes(1);  // +1 minuto de tolerancia

        // Act
        var token = _jwtService.GenerateToken();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.NotNull(jwtToken.ValidTo);
        Assert.True(jwtToken.ValidTo > expectedMinExpiration, 
            $"Token expiration {jwtToken.ValidTo} should be after {expectedMinExpiration}");
        Assert.True(jwtToken.ValidTo < expectedMaxExpiration, 
            $"Token expiration {jwtToken.ValidTo} should be before {expectedMaxExpiration}");
    }

    [Fact]
    public void GenerateToken_HasIssuedAtTime()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
        var expectedMinIssuedAt = DateTime.UtcNow.AddMinutes(-1); // -1 minuto de tolerancia
        var expectedMaxIssuedAt = DateTime.UtcNow.AddMinutes(1);  // +1 minuto de tolerancia

        // Act
        var token = _jwtService.GenerateToken();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.NotNull(jwtToken.IssuedAt);
        Assert.True(jwtToken.IssuedAt > expectedMinIssuedAt, 
            $"Token issued at {jwtToken.IssuedAt} should be after {expectedMinIssuedAt}");
        Assert.True(jwtToken.IssuedAt < expectedMaxIssuedAt, 
            $"Token issued at {jwtToken.IssuedAt} should be before {expectedMaxIssuedAt}");
    }
}
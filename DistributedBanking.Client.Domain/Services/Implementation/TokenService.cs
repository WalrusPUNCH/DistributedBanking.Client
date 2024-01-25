using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contracts.Constants;
using DistributedBanking.Client.Domain.Models.Identity;
using DistributedBanking.Client.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DistributedBanking.Client.Domain.Services.Implementation;

public class TokenService : ITokenService
{
    private readonly IUserManager _userManager;
    private readonly JwtOptions _jwtOptions;

    public TokenService(
        IUserManager userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<string> GenerateTokenAsync(UserModel user)
    {
        var roles = await _userManager.GetRolesAsync(user.Id);
        var jwtSecurityToken = CreateJwtToken(roles, user.Email, user.EndUserId);
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        return token;
    }
    
    private JwtSecurityToken CreateJwtToken(
        IEnumerable<string> roles,
        string email,
        string endUserId)
    {
        var roleClaims = roles.Select(t => new Claim("roles", t)).ToList();
        
        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimConstants.UserIdClaim, endUserId)
            }
            .Union(roleClaims);
        
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signingCredentials);
        
        return jwtSecurityToken;
    }
}
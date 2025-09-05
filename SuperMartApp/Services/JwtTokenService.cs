using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SuperMartApp.Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace SuperMartApp.Web.Services
{

    public interface IJwtTokenService
    {
        Task<(string token, DateTime expires)> GenerateAccessTokenAsync(AppUser user);
        string GenerateRefreshToken();
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _cfg;
        private readonly UserManager<AppUser> _um;

        public JwtTokenService(IConfiguration cfg, UserManager<AppUser> um)
        {
            _cfg = cfg; _um = um;
        }

        public async Task<(string token, DateTime expires)> GenerateAccessTokenAsync(AppUser user)
        {
            var jwt = _cfg.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _um.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"]!));
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public string GenerateRefreshToken()
        {
            // 256-bit token, URL-safe
            var bytes = new byte[32];
            Random.Shared.NextBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }

}



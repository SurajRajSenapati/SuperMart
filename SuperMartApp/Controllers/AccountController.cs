using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Web.Services;
using SuperMartApp.Infrastructure.Data;
using SuperMartApp.Domain.Entities;
using SuperMartApp.Web.Entities;

namespace SuperMartApp.Web.Controllers.Api
{

    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _um;
        private readonly SignInManager<AppUser> _sm;
        private readonly IJwtTokenService _jwt;
        private readonly IConfiguration _cfg;
        private readonly AppDbContext _db;

        public AccountController(UserManager<AppUser> um, SignInManager<AppUser> sm, IJwtTokenService jwt, IConfiguration cfg, AppDbContext db)
        {
            _um = um; _sm = sm; _jwt = jwt; _cfg = cfg; _db = db;
        }



        // POST: /api/auth/register  (lock down to Admin later if you want)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new AppUser { UserName = dto.UserName, Email = dto.Email };
            var res = await _um.CreateAsync(user, dto.Password);
            if (!res.Succeeded) return BadRequest(res.Errors);
            // Optional: role
            if (!string.IsNullOrWhiteSpace(dto.Role))
                await _um.AddToRoleAsync(user, dto.Role);

            return Ok(new { user.Id, user.UserName, user.Email });
        }



        // POST: /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _um.FindByNameAsync(dto.UserName)
                       ?? await _um.FindByEmailAsync(dto.UserName);
            if (user is null) return Unauthorized("Invalid credentials");

            var ok = await _sm.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!ok.Succeeded) return Unauthorized("Invalid credentials");

            var (access, expires) = await _jwt.GenerateAccessTokenAsync(user);
            var refresh = _jwt.GenerateRefreshToken();

            var days = int.Parse(_cfg["Jwt:RefreshTokenDays"]!);
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                Revoked = false
            });
            await _db.SaveChangesAsync();

            var roles = await _um.GetRolesAsync(user);
            return Ok(new TokenResponseDto
            {
                AccessToken = access,
                ExpiresAtUtc = expires,
                RefreshToken = refresh,
                User = new UserDto { Id = user.Id, UserName = user.UserName ?? "", Roles = roles.ToArray() }
            });
        }



        // POST: /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshDto dto)
        {
            var record = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);
            if (record is null || record.Revoked || record.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var user = await _um.FindByIdAsync(record.UserId);
            if (user is null) return Unauthorized();

            var (access, expires) = await _jwt.GenerateAccessTokenAsync(user);
            return Ok(new { accessToken = access, expiresAtUtc = expires });
        }



        // POST: /api/auth/logout  (revokes a refresh token)
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout(RefreshDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Unauthorized();

            var record = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == dto.RefreshToken && x.UserId == userId);
            if (record is null) return NotFound();
            record.Revoked = true;
            await _db.SaveChangesAsync();
            return Ok();
        }



        // GET: /api/auth/me  (JWT only)
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
            var user = await _um.FindByIdAsync(userId);
            var roles = await _um.GetRolesAsync(user!);
            return Ok(new UserDto { Id = userId, UserName = user!.UserName ?? "", Roles = roles.ToArray() });
        }

        public class RegisterDto { public string UserName { get; set; } = default!; public string Email { get; set; } = default!; public string Password { get; set; } = default!; public string? Role { get; set; } }
        public class LoginDto { public string UserName { get; set; } = default!; public string Password { get; set; } = default!; }
        public class RefreshDto { public string RefreshToken { get; set; } = default!; }
        public class UserDto { public string Id { get; set; } = default!; public string UserName { get; set; } = default!; public string[] Roles { get; set; } = Array.Empty<string>(); }
        public class TokenResponseDto
        {
            public string AccessToken { get; set; } = default!;
            public DateTime ExpiresAtUtc { get; set; }
            public string RefreshToken { get; set; } = default!;
            public UserDto User { get; set; } = default!;
        }



    }
}

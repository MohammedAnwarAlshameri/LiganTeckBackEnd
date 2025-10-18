using Application.DTOs;
using Application.IServices;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AdminUserService(ApplicationDbContext db, IJwtTokenService jwt)
        {
            _db = db; _jwt = jwt;
        }

        public async Task<LoginResponseDto> LoginAsync(AdminLoginRequest req, CancellationToken ct = default)
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var user = await _db.AdminUser
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password ?? "", user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            // الدور ثابت: Admin (بدون Staff/Support/Finance)
            var token = _jwt.CreateToken(
                userId: user.AdminUserId,
                email: user.Email,
                role: "Admin",
                name: user.DisplayName ?? user.Email,
                extraClaims: new Dictionary<string, string>
                {
                    ["user_type"] = "Admin"
                });

            return new LoginResponseDto
            {
                Token = token,
                TenantId = 0,
                Email = user.Email,
                TenantName = user.DisplayName ?? user.Email
            };
        }
    }
}

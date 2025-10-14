using Application.DTOs;
using Application.IServices;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AdminUserService: IAdminUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AdminUserService(ApplicationDbContext db, IJwtTokenService jwt)
        { _db = db; _jwt = jwt; }

        public async Task<LoginResponseDto> LoginAsync(AdminLoginRequest req, CancellationToken ct = default)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var u = await _db.AdminUser
                    .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted, ct)
                    ?? throw new InvalidOperationException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password, u.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            // لاحظ أننا نمرر الدور من قاعدة البيانات
            var token = _jwt.CreateToken(
                userId: u.AdminUserId,
                email: u.Email,
                role: u.RoleName,        // Admin | Support | Finance
                name: u.DisplayName ?? u.Email,
                extraClaims: new Dictionary<string, string> { ["user_type"] = "staff" }
            );

            return new LoginResponseDto { Token = token, TenantId = 0, Email = u.Email, TenantName = u.DisplayName ?? u.Email };
        }
    }
}

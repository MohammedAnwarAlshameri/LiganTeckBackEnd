using Application.DTOs;
using Application.IServices;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Auth
{
    public sealed class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AuthService(ApplicationDbContext db, IJwtTokenService jwt)
        {
            _db = db; _jwt = jwt;
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto req, CancellationToken ct = default)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (await _db.Set<Tenant>().AnyAsync(t => t.TenantEmail == email && !t.IsDeleted, ct))
                throw new InvalidOperationException("Email already exists.");

            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var tenant = new Tenant
            {
                TenantName = req.TenantName.Trim(),
                TenantEmail = email,
                TenantPassword = hash,
                CountryCode = (req.CountryCode ?? "SA").Trim().ToUpperInvariant(),
                TenantStatusid = 1, // Active
                IsDeleted = false
            };

            _db.Add(tenant);
            await _db.SaveChangesAsync(ct);

            var token = _jwt.CreateToken(tenant.TenantId, tenant.TenantEmail, tenant.TenantName);
            return new LoginResponseDto { Token = token, TenantId = tenant.TenantId, Email = tenant.TenantEmail, TenantName = tenant.TenantName };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto req, CancellationToken ct = default)
        {
            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();

            // نقرأ فقط الأعمدة اللازمة لتسجيل الدخول
            var row = await _db.Set<Tenant>()
                .AsNoTracking()
                .Where(t => t.TenantEmail == email && !t.IsDeleted)
                .Select(t => new
                {
                    t.TenantId,
                    Email = t.TenantEmail,
                    Name = t.TenantName,
                    PasswordHash = t.TenantPassword,
                    Status = t.TenantStatusid
                })
                .SingleOrDefaultAsync(ct);

            // إخفاء تفاصيل السبب لأمان أعلى
            if (row is null || string.IsNullOrEmpty(row.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            // لو عندك حالة للحساب (مثلاً 1 = Active)
            if (row.Status != 1)
                throw new InvalidOperationException("Account is inactive.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password ?? string.Empty, row.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            var token = _jwt.CreateToken(
                row.TenantId,
                row.Email ?? string.Empty,
                row.Name ?? string.Empty
            );

            return new LoginResponseDto
            {
                Token = token,
                TenantId = row.TenantId,
                Email = row.Email ?? string.Empty,
                TenantName = row.Name ?? string.Empty
            };
        }

    }
}

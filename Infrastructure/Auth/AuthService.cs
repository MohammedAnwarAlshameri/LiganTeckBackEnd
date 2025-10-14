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
            var email = req.Email.Trim().ToLowerInvariant();
            var tenant = await _db.Set<Tenant>().FirstOrDefaultAsync(t => t.TenantEmail == email && !t.IsDeleted, ct)
                         ?? throw new InvalidOperationException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password, tenant.TenantPassword))
                throw new InvalidOperationException("Invalid credentials.");

            var token = _jwt.CreateToken(tenant.TenantId, tenant.TenantEmail, tenant.TenantName);
            return new LoginResponseDto { Token = token, TenantId = tenant.TenantId, Email = tenant.TenantEmail, TenantName = tenant.TenantName };
        }
    }
}

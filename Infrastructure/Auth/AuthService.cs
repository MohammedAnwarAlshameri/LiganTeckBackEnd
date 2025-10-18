using Application.DTOs;
using Application.IServices;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

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
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var username = (req.Username ?? "").Trim();
            var phone = (req.PhoneNumber ?? "").Trim();
            var tenantName = (req.TenantName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(req.Password)) throw new ArgumentException("Password is required.");
            if (string.IsNullOrWhiteSpace(tenantName)) throw new ArgumentException("TenantName is required.");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("UserName is required.");

            // TenantName unique (case-insensitive, مع تجاهل المحذوفين)
            if (await _db.Set<Tenant>().AnyAsync(
                t => !t.IsDeleted && t.TenantName.ToLower() == tenantName.ToLower(), ct))
                throw new InvalidOperationException("Tenant name already exists.");

            // Email unique
            if (await _db.Set<Tenant>().AnyAsync(
                t => !t.IsDeleted && t.TenantEmail == email, ct))
                throw new InvalidOperationException("Email already exists.");

            // Username unique 
            if (!string.IsNullOrWhiteSpace(username))
            {
                var uname = username.ToLowerInvariant();
                if (await _db.Set<Tenant>().AnyAsync(
                    t => !t.IsDeleted && t.Username.ToLower() == uname, ct))
                    throw new InvalidOperationException("Username already exists.");
            }

            // يُسمح بالتكرار
            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var tenant = new Tenant
            {
                TenantName = tenantName,
                TenantEmail = email,
                TenantPassword = hash,
                CountryCode = (req.CountryCode ?? "SA").Trim().ToUpperInvariant(),
                Username = string.IsNullOrWhiteSpace(username) ? email : username,
                PhoneNumber = phone,
                TenantStatusid = 1,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            };

            _db.Add(tenant);
            await _db.SaveChangesAsync(ct);

            var token = _jwt.CreateToken(tenant.TenantId, tenant.TenantEmail, tenant.TenantName);
            return new LoginResponseDto
            {
                Token = token,
                TenantId = tenant.TenantId,
                Email = tenant.TenantEmail,
                TenantName = tenant.TenantName
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto req, CancellationToken ct = default)
        {
            var input = (req.Email ?? "").Trim();
            var pwd = req.Password ?? "";

            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(pwd))
                throw new InvalidOperationException("Invalid credentials.");

            var email = input.ToLowerInvariant();
            var username = input;
            var phone = input;

            var row = await _db.Set<Tenant>()
                .AsNoTracking()
                .Where(t => !t.IsDeleted &&
                           (t.TenantEmail == email || t.Username == username || t.PhoneNumber == phone))
                .Select(t => new
                {
                    t.TenantId,
                    Email = t.TenantEmail,
                    Name = t.TenantName,
                    PasswordHash = t.TenantPassword,
                    Status = t.TenantStatusid
                })
                .SingleOrDefaultAsync(ct);

            if (row is null || string.IsNullOrEmpty(row.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");
            if (row.Status != 1)
                throw new InvalidOperationException("Account is inactive.");
            if (!BCrypt.Net.BCrypt.Verify(pwd, row.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            var token = _jwt.CreateToken(
                tenantId: row.TenantId,
                email: row.Email ?? string.Empty,
                tenantName: row.Name ?? string.Empty
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

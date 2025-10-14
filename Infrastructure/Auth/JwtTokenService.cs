using Application.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Auth
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _cfg;
        public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

        // لعملاء Tenant (Customer)
        public string CreateToken(long tenantId, string email, string? tenantName = null)
            => CreateToken(userId: tenantId, email: email, role: "Customer", name: tenantName,
                           extraClaims: new Dictionary<string, string> { ["user_type"] = "tenant", ["tenant_id"] = tenantId.ToString() });

        // عام: نحدد الدور صراحة (للإدارة)
        public string CreateToken(long userId, string email, string role, string? name = null, IDictionary<string, string>? extraClaims = null)
        {
            var keyStr = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name ?? email),
            new(ClaimTypes.Role, role)
        };

            if (extraClaims != null)
                foreach (var kv in extraClaims)
                    claims.Add(new Claim(kv.Key, kv.Value));

            var minutes = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       
    }
}

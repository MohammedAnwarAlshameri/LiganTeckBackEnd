using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class RegisterRequestDto
    {
        public string TenantName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string CountryCode { get; set; } = "SA";
        public string Username { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }

    public sealed class LoginRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public sealed class LoginResponseDto
    {
        public string Token { get; init; } = null!;
        public long TenantId { get; init; }
        public string Email { get; init; } = null!;
        public string TenantName { get; init; } = null!;
    }
}

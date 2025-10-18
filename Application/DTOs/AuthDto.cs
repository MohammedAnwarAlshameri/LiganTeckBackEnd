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
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
       // public string? FullName { get; set; } 

    }

    public  class LoginRequestDto
    {
        // يمكن أن يكون Email أو Username أو Phone
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string Token { get; init; } = null!;
        public long TenantId { get; init; }
        public string Email { get; init; } = null!;
        public string TenantName { get; init; } = null!;
    }
}

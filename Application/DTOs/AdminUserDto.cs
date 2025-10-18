using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class AdminLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    // لو أردت معلومات إضافية في رد الأدمن
    public sealed class AdminLoginResponseDto : LoginResponseDto
    {
        public string Role { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public string UserType { get; init; } = "staff";
    }
}

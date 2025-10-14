using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto req, CancellationToken ct = default);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto req, CancellationToken ct = default);
    }
}

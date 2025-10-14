using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IJwtTokenService
    {
        string CreateToken(long tenantId, string email, string? tenantName = null);
        string CreateToken(long userId, string email, string role, string name, IDictionary<string, string> extraClaims);
    }
}

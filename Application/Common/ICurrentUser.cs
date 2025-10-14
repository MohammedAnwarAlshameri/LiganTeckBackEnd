using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        long? TenantId { get; }
        string? Email { get; }
        string? Name { get; }
    }
}

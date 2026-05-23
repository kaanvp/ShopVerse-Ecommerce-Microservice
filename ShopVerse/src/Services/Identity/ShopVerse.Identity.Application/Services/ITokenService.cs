using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Services
{
    public interface ITokenService
    {
        string GenerateAccesToken(ClaimsPrincipal user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token);
    }

}

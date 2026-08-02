using System.Security.Claims;

namespace Backend.Services
{
    public static class ClaimHelper
    {
        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? user.FindFirst("sub")?.Value 
                ?? user.FindFirst("nameid")?.Value;
        }

        public static string? GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value 
                ?? user.FindFirst("role")?.Value;
        }

        public static string? GetColegioId(ClaimsPrincipal user)
        {
            return user.FindFirst("ColegioId")?.Value 
                ?? user.FindFirst("colegioid")?.Value;
        }
    }
}

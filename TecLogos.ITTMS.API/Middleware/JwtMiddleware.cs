using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TecLogos.ITTMS.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: Implement JWT validation and set context.User
            await _next(context);
        }
    }
}

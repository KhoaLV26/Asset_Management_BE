using AssetManagement.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Middlewares
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, DBContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var blacklistedToken = await dbContext.BlackListTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (blacklistedToken != null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
    }
}
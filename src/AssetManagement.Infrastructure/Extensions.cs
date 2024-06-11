using AssetManagement.Infrastructure.Services;
using AssetManagement.Infrastructure.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AssetManagement.Infrastructure
{
    public static class Extensions
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
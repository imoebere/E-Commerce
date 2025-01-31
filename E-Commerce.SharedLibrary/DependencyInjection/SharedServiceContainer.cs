using E_Commerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Serilog;

namespace E_Commerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services,
           IConfiguration config, string fileName) where TContext : DbContext
        {
            // Add Generic Database context
            services.AddDbContext<TContext>(options => options.UseSqlServer(
                config.GetConnectionString("eCommerceConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

            // Configure serialog logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();


            // Add JWT Authentication Scheme
            AuthenticationSheme.AddAuthenticationScheme(services, config);
            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            // Use global Exception
            app.UseMiddleware<GlobalException>();

            // Register middleware to block outside Api Calls
            app.UseMiddleware<ListenToApiGateway>();
            return app;


        }
    }
}

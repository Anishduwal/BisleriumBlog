using BisleriumBlog.Infrastructure.Implementations.Services;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using BisleriumBlog.Infrastructure.Persistence;
using BisleriumBlog.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace BisleriumBlog.Dependency;

public static class InfrastructureService
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("Bislerium")));

        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IMailService, MailService>();
        services.AddTransient<IFileUploadService, FileUploadService>();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityService.Infrastructure.Persistence;

public class ApplicationIdentityDbContextFactory : IDesignTimeDbContextFactory<ApplicationIdentityDbContext>
{
    public ApplicationIdentityDbContext CreateDbContext(string[] args)
    {
        string? connectionString = null;
        
        // get the connect from the args first
        var connArgIndex = Array.IndexOf(args, "--connectionString");
        if (connArgIndex >= 0 && connArgIndex + 1 < args.Length)
        {
            connectionString = args[connArgIndex + 1];
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FamilySafetyDbContext>
{
    private const string DefaultDevelopmentConnection =
        "Server=localhost;Database=FamilySafety;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public FamilySafetyDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__FamilySafety")
            ?? DefaultDevelopmentConnection;
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new FamilySafetyDbContext(options);
    }
}

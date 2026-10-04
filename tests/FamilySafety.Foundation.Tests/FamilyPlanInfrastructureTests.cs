using FamilySafety.Domain.Plans;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyPlanInfrastructureTests
{
    [Fact]
    public void PlanAndAudienceTablesAreInTheModelAndMigration()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);

        Assert.Contains(typeof(FamilyPlan), dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType));
        Assert.Contains(
            typeof(FamilyPlanAudienceMember),
            dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType));
        Assert.Equal(
            ["PlanId", "ProfileId"],
            dbContext.Model.FindEntityType(typeof(FamilyPlanAudienceMember))!
                .FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_PlanAudienceSelection", StringComparison.Ordinal));

        var script = dbContext.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [FamilyPlans]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [FamilyPlanAudienceMembers]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ON DELETE NO ACTION", script, StringComparison.OrdinalIgnoreCase);
    }
}

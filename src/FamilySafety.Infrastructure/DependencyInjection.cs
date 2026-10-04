using FamilySafety.Application.Directory;
using FamilySafety.Application.Events;
using FamilySafety.Application.Invitations;
using FamilySafety.Application.Profiles;
using FamilySafety.Application.Plans;
using FamilySafety.Application.Trips;
using FamilySafety.Application.Relationships;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Incidents;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySafety.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFamilySafetyInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<FamilySafetyDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAdultProfileRepository, EfAdultProfileRepository>();
        services.AddScoped<IMembershipInvitationRepository, EfMembershipInvitationRepository>();
        services.AddScoped<IFamilyDirectoryRepository, EfFamilyDirectoryRepository>();
        services.AddScoped<IFamilyEventRepository, EfFamilyEventRepository>();
        services.AddScoped<IFamilyPlanRepository, EfFamilyPlanRepository>();
        services.AddScoped<IFamilyTripRepository, EfFamilyTripRepository>();
        services.AddScoped<IFamilyTripNoteRepository, EfFamilyTripNoteRepository>();
        services.AddScoped<IFamilyRelationshipRepository, EfFamilyRelationshipRepository>();
        services.AddScoped<IConcernRepository, EfConcernRepository>();
        services.AddScoped<IConcernAccessQuery, EfConcernAccessQuery>();
        services.AddScoped<IIncidentRepository, EfIncidentRepository>();
        return services;
    }
}

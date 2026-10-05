using System.Text.Json.Serialization;
using FamilySafety.Api.CheckIns;
using FamilySafety.Api.Sharing;
using FamilySafety.Application.Sharing;
using FamilySafety.Application.CheckIns;
using FamilySafety.Api.Development;
using FamilySafety.Api.Directory;
using FamilySafety.Api.FamilyDates;
using FamilySafety.Application.FamilyDates;
using FamilySafety.Api.Events;
using FamilySafety.Api.Invitations;
using FamilySafety.Api.Profiles;
using FamilySafety.Api.Plans;
using FamilySafety.Api.Trips;
using FamilySafety.Api.Relationships;
using FamilySafety.Api.Concerns;
using FamilySafety.Api.Incidents;
using FamilySafety.Application.Directory;
using FamilySafety.Application.Events;
using FamilySafety.Application.Invitations;
using FamilySafety.Application.Profiles;
using FamilySafety.Application.Plans;
using FamilySafety.Application.Trips;
using FamilySafety.Application.Relationships;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Incidents;
using FamilySafety.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("FamilySafety")
    ?? throw new InvalidOperationException("Connection string 'FamilySafety' is required.");

builder.Services.AddFamilySafetyInfrastructure(connectionString);
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<SharingSessionService>();
builder.Services.AddScoped<IAdultProfileService, AdultProfileService>();
builder.Services.AddScoped<IMembershipInvitationService, MembershipInvitationService>();
builder.Services.AddScoped<IFamilyDirectoryService, FamilyDirectoryService>();
builder.Services.AddScoped<ISharedFamilyDateService, SharedFamilyDateService>();
builder.Services.AddScoped<IFamilyEventService, FamilyEventService>();
builder.Services.AddScoped<IEventInvitationService, EventInvitationService>();
builder.Services.AddScoped<IEventNeedService, EventNeedService>();
builder.Services.AddScoped<IEventUpdateService, EventUpdateService>();
builder.Services.AddScoped<IFamilyPlanService, FamilyPlanService>();
builder.Services.AddScoped<IFamilyTripService, FamilyTripService>();
builder.Services.AddScoped<IFamilyTripNoteService, FamilyTripNoteService>();
builder.Services.AddScoped<IFamilyRelationshipService, FamilyRelationshipService>();
builder.Services.AddScoped<IConcernService, ConcernService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddSingleton(TimeProvider.System);
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<ConcernMaintenanceService>();
    builder.Services.AddHostedService<IncidentMaintenanceService>();
    builder.Services.AddHostedService<SharedFamilyDateMaintenanceService>();
    builder.Services.AddHostedService<SharingSessionMaintenanceService>();
}
builder.Services.AddCurrentAccount(builder.Environment, builder.Configuration);
builder.Services.AddDevelopmentFixtures(builder.Environment, builder.Configuration);
builder.Services.AddDevelopmentInvitationOutbox(builder.Environment, builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.MapHealthChecks("/health");
app.MapCheckInEndpoints();
app.MapSharingSessionEndpoints();
app.MapAdultProfileEndpoints();
app.MapMembershipInvitationEndpoints();
app.MapFamilyDirectoryEndpoints();
app.MapSharedFamilyDateEndpoints();
app.MapFamilyEventEndpoints();
app.MapEventInvitationEndpoints();
app.MapEventNeedEndpoints();
app.MapEventUpdateEndpoints();
app.MapFamilyPlanEndpoints();
app.MapFamilyTripEndpoints();
app.MapFamilyTripNoteEndpoints();
app.MapFamilyRelationshipEndpoints();
app.MapConcernEndpoints();
app.MapIncidentEndpoints();
app.MapDevelopmentInvitationOutbox(app.Environment, app.Configuration);
app.MapDevelopmentIdentities(app.Environment, app.Configuration);

app.Run();

public partial class Program;

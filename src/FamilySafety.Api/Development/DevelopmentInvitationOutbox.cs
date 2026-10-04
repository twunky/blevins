using System.Collections.Concurrent;
using FamilySafety.Application.Invitations;

namespace FamilySafety.Api.Development;

public sealed record DevelopmentInvitationMessage(
    string RecipientEmail,
    string InvitationUrl,
    DateTimeOffset CreatedAt);

public sealed class DevelopmentInvitationOutbox(string clientBaseUrl, TimeProvider timeProvider)
    : IMembershipInvitationDelivery
{
    private readonly ConcurrentQueue<DevelopmentInvitationMessage> messages = new();
    private readonly string clientBaseUrl = clientBaseUrl.TrimEnd('/');

    public IReadOnlyList<DevelopmentInvitationMessage> Messages => messages.ToArray();

    public Task SendAsync(string recipientEmail, string token, CancellationToken cancellationToken)
    {
        messages.Enqueue(new DevelopmentInvitationMessage(
            recipientEmail,
            $"{clientBaseUrl}/#invite={Uri.EscapeDataString(token)}",
            timeProvider.GetUtcNow()));
        return Task.CompletedTask;
    }
}

public static class DevelopmentInvitationOutboxRegistration
{
    public static IServiceCollection AddDevelopmentInvitationOutbox(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentInvitationOutbox:Enabled");
        DevelopmentOnlyFeatureGuard.EnsureAllowed(environment.IsDevelopment(), enabled, "Development invitation outbox");
        if (!enabled)
        {
            services.AddSingleton<IMembershipInvitationDelivery, UnavailableInvitationDelivery>();
            return services;
        }

        var clientBaseUrl = configuration["DevelopmentInvitationOutbox:ClientBaseUrl"];
        if (!Uri.TryCreate(clientBaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("DevelopmentInvitationOutbox:ClientBaseUrl must be an absolute URL.");
        }

        services.AddSingleton(provider => new DevelopmentInvitationOutbox(
            clientBaseUrl!, provider.GetRequiredService<TimeProvider>()));
        services.AddSingleton<IMembershipInvitationDelivery>(provider =>
            provider.GetRequiredService<DevelopmentInvitationOutbox>());
        return services;
    }

    public static IEndpointRouteBuilder MapDevelopmentInvitationOutbox(
        this IEndpointRouteBuilder endpoints,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentInvitationOutbox:Enabled");
        DevelopmentOnlyFeatureGuard.EnsureAllowed(environment.IsDevelopment(), enabled, "Development invitation outbox endpoint");
        if (enabled)
        {
            endpoints.MapGet("/api/development/invitation-outbox", (DevelopmentInvitationOutbox outbox) =>
                Results.Ok(outbox.Messages));
        }

        return endpoints;
    }

    private sealed class UnavailableInvitationDelivery : IMembershipInvitationDelivery
    {
        public Task SendAsync(string recipientEmail, string token, CancellationToken cancellationToken) =>
            throw new MembershipInvitationDeliveryUnavailableException();
    }
}

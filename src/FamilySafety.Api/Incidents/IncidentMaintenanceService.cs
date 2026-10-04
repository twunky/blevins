using FamilySafety.Application.Incidents;

namespace FamilySafety.Api.Incidents;

/// <summary>
/// Runs Incident reconciliation at startup and then every two minutes: materializes due freezes and
/// grant revocations, deletes expired Incident aggregates, and deletes audit rows past retention.
/// Correctness never depends on this service; request-time checks already apply the same rules.
/// </summary>
public sealed partial class IncidentMaintenanceService(IServiceScopeFactory scopes,
    ILogger<IncidentMaintenanceService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IIncidentRepository>()
                    .ReconcileAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogReconcileFailure(logger, exception);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Incident maintenance reconciliation failed.")]
    private static partial void LogReconcileFailure(ILogger logger, Exception exception);
}

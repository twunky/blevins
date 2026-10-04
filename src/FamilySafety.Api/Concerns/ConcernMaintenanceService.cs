using FamilySafety.Application.Concerns;

namespace FamilySafety.Api.Concerns;

public sealed partial class ConcernMaintenanceService(IServiceScopeFactory scopes,
    ILogger<ConcernMaintenanceService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IConcernRepository>()
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Concern maintenance reconciliation failed.")]
    private static partial void LogReconcileFailure(ILogger logger, Exception exception);
}

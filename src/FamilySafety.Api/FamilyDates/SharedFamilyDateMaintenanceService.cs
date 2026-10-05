using FamilySafety.Application.FamilyDates;

namespace FamilySafety.Api.FamilyDates;

public sealed partial class SharedFamilyDateMaintenanceService(IServiceScopeFactory scopes,
    ILogger<SharedFamilyDateMaintenanceService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ISharedFamilyDateRepository>().ReconcileAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { LogCleanupFailure(logger, exception); }
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
    [LoggerMessage(Level = LogLevel.Error, Message = "Shared Family Date maintenance failed.")]
    private static partial void LogCleanupFailure(ILogger logger, Exception exception);
}

using FamilySafety.Application.Sharing;

namespace FamilySafety.Api.Sharing;

public sealed partial class SharingSessionMaintenanceService(IServiceScopeFactory scopes,
    ILogger<SharingSessionMaintenanceService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ISharingSessionRepository>().CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { LogCleanupFailure(logger, exception); }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
    [LoggerMessage(Level = LogLevel.Error, Message = "Sharing Session current-location cleanup failed.")]
    private static partial void LogCleanupFailure(ILogger logger, Exception exception);
}

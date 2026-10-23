namespace WineApp.Services;

public sealed class AuditRetentionWorker(IServiceScopeFactory scopes, TimeProvider clock,
    ILogger<AuditRetentionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24), clock);
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<AuditRetentionService>().RunScheduledAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Scheduled audit retention did not complete; history may require operator review"); }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
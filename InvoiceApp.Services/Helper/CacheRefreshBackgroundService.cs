using InvoiceApp.Services.IServices;
using InvoiceApp.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceApp.Services.Helper
{
    public class CacheRefreshBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheRefreshBackgroundService> _logger;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(30); // Adjust as needed

        public CacheRefreshBackgroundService(IServiceProvider serviceProvider, ILogger<CacheRefreshBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cache refresh operation started.");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var invoiceIdService = scope.ServiceProvider.GetRequiredService<IInvoiceIdService>();
                    try
                    {
                        await invoiceIdService.RefreshCacheAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the cache refresh operation.");
                    }
                }

                await Task.Delay(_refreshInterval, stoppingToken);
            }
        }
    }
}

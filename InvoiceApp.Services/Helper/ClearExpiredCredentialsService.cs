
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Data.Models.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceApp.Services.Helper
{
    public class ClearExpiredCredentialsService : BackgroundService
    {
        private readonly ILogger<ClearExpiredCredentialsService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ClearExpiredCredentialsService(ILogger<ClearExpiredCredentialsService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRunTime = now.Date.AddDays(1); 
                var delay = nextRunTime - now;

                _logger.LogInformation($"ClearExpiredCredentialsService scheduled to run at: {nextRunTime}. Current time: {now}.");
                await Task.Delay(delay, stoppingToken);

                // It's now close to midnight, time to run the task
                await ClearExpiredCredentialsAsync();

                // Wait a bit before next calculation to ensure we're past midnight/day boundary
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ClearExpiredCredentialsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var unitOfWork = scopedServices.GetRequiredService<IUnitOfWork>();

                _logger.LogInformation("Starting to clear expired credentials.");

                try
                {
                    // Implementation to delete expired credentials
                    await unitOfWork.SwaggerCredentialRepository.GetExpiredSwaggerCredentialsAsync();
                    await unitOfWork.SaveAsync(CancellationToken.None);

                    _logger.LogInformation("Expired credentials cleared successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while clearing expired credentials.");
                }
            }
        }
    }
}

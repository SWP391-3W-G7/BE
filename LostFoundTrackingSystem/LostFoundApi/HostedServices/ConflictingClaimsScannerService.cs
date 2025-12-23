using BLL.IServices;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LostFoundApi.HostedServices
{
    public class ConflictingClaimsScannerService : IHostedService, IDisposable
    {
        private readonly ILogger<ConflictingClaimsScannerService> _logger;
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public ConflictingClaimsScannerService(ILogger<ConflictingClaimsScannerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Conflicting Claims Scanner Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60)); // Run once an hour
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Conflicting Claims Scanner Service is working.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var claimRequestService = scope.ServiceProvider.GetRequiredService<IClaimRequestService>();
                try
                {
                    await claimRequestService.ScanForConflictingClaimsAsync();
                    _logger.LogInformation("Successfully scanned for conflicting claims.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while scanning for conflicting claims.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Conflicting Claims Scanner Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using BLL.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LostFoundApi.HostedServices
{
    public class MatchingHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public MatchingHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run every 30 seconds
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(120));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
                matchingService.FindAndCreateMatchesForAllLostItemsAsync().GetAwaiter().GetResult();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

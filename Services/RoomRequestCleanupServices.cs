using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectEstágio.Data;

namespace ProjectEstagio.Services
{
    public class RoomRequestCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RoomRequestCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Executa uma vez por dia

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var cutoffDate = DateTime.Now.Date.AddDays(-30);
                    var oldRequests = context.RoomRequests
                        .Where(r => r.Date < cutoffDate)
                        .ToList();

                    if (oldRequests.Any())
                    {
                        context.RoomRequests.RemoveRange(oldRequests);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}

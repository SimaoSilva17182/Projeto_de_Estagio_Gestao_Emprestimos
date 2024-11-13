using ProjectEstágio.Data;

namespace ProjectEstagio.Services
{
    public class VeiculeRequestCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public VeiculeRequestCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var cutoffDate = DateTime.Now.Date.AddDays(-30);
                    var oldRequests = context.VeiculeRequests
                        .Where(r => r.EndDate < cutoffDate)
                        .ToList();

                    if (oldRequests.Any())
                    {
                        context.VeiculeRequests.RemoveRange(oldRequests);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }

}

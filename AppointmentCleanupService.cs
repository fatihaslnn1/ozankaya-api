using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AppointmentCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AppointmentCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(1).AddHours(1); // Gece saat 01:00
            
            if (now.Hour < 1)
            {
                nextRun = DateTime.Today.AddHours(1);
            }

            var delay = nextRun - now;
            if (delay.TotalMilliseconds < 0)
            {
                nextRun = nextRun.AddDays(1);
                delay = nextRun - now;
            }

            // Gece saat 01:00'a kadar arka planda bekle
            await Task.Delay(delay, stoppingToken);

            // Saat 01:00 olunca çalışacak temizlik kodu
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); 
                    var todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                    // Sadece geçmiş tarihli VE onaylanmış (IsAccepted == true) randevuları seç
                    var oldAppointments = db.Appointments
                        .Where(a => string.Compare(a.Date, todayStr) < 0 && a.IsAccepted == true)
                        .ToList();

                    if (oldAppointments.Any())
                    {
                        db.Appointments.RemoveRange(oldAppointments);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Otomatik silme hatası: {ex.Message}");
            }
        }
    }
}
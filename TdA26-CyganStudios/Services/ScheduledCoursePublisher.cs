using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Services;

public sealed class ScheduledCoursePublisher : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private Timer? _timer;

    public ScheduledCoursePublisher(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // run immediately, then every minute
        _timer = new Timer(async _ => await DoWork(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var due = await db.Courses
                .Where(c => c.State == CourseState.Draft &&
                            c.ScheduledPublishAt.HasValue &&
                            c.ScheduledPublishAt <= now)
                .ToListAsync();

            if (due.Count == 0)
                return;

            foreach (var c in due)
            {
                c.State = CourseState.Published;
                c.ScheduledPublishAt = null;
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // swallow - nothing we can do, maybe log if you have logger here later
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
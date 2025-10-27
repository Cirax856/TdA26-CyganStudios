using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace TdA26_CyganStudios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .CreateLogger();

            Log.Logger = log;

            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
                {
                    Log.Fatal($"Unhandeled exception: {e.ExceptionObject}");
                    Log.CloseAndFlush();
                    Environment.Exit(1);
                };
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSerilog();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=app.db"));

            var app = builder.Build();

            app.UseSerilogRequestLogging();
            
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.MapControllers();

            Log.Information("Updating db...");
            using (var scope = app.Services.CreateScope())
            {
                var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                appDb.Database.Migrate();
            }

            Log.Information("Updated db");

            app.Run();
        }
    }
}

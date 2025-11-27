using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace TdA26_CyganStudios;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var log = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", formatProvider: CultureInfo.InvariantCulture)
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
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            //options.AccessDeniedPath = "/access-denied";
        });

        builder.Services.AddHttpClient("course_material_verify", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        });

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.MapControllers();

        Log.Information("Updating db...");
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var appDb = services.GetRequiredService<AppDbContext>();

                await appDb.Database.MigrateAsync();

                await DataSeeder.SeedDefaultUserAsync(services);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred during database migration or seeding.");
                throw;
            }
        }

        Log.Information("Updated db");

        await app.RunAsync();
    }
}

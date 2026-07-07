using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSM.GS
{
    /// <summary>
    ///     This is the UDP hole punching server, the clients will connect
    ///     to this server to setup the correct ports. It also exposes a small
    ///     HTTP endpoint for the mod's update/version check. The public server
    ///     list itself is served over the UDP protocol (ServerListRequestCommand/
    ///     ServerListResultCommand), not HTTP.
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("settings.json", true, true);
            builder.Configuration.AddEnvironmentVariables("GS_");
            builder.Configuration.AddCommandLine(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddSingleton<WorkerService>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkerService>());

            int httpPort = int.TryParse(builder.Configuration.GetSection("HTTP_PORT").Value, out int val) ? val : 4241;
            builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}");

            WebApplication app = builder.Build();

            // The mod pings this to check for updates and to validate a custom
            // API server URL entered in settings (CSMWebClient just needs the
            // request to succeed; the version itself isn't otherwise used here).
            app.MapGet("/api/version", () => "v0.0");

            app.Run();
        }
    }
}

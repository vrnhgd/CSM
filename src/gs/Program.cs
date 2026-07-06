using System.Linq;
using System.Security.Principal;
using CSM.GS.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSM.GS
{
    /// <summary>
    ///     This is the UDP hole punching server, the clients will connect
    ///     to this server to setup the correct ports. It also exposes an
    ///     HTTP endpoint listing servers that have opted in to public listing.
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

            // Keep PascalCase property names (disable the default camelCase policy) since
            // the mod parses this JSON with Unity's JsonUtility, which matches field names
            // case-sensitively against the C# field names (Name, CurrentPlayers, etc.).
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = null;
            });

            int httpPort = int.TryParse(builder.Configuration.GetSection("HTTP_PORT").Value, out int val) ? val : 4241;
            builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}");

            WebApplication app = builder.Build();

            // The mod pings this to check for updates and to validate a custom
            // API server URL entered in settings (CSMWebClient just needs the
            // request to succeed; the version itself isn't otherwise used here).
            app.MapGet("/api/version", () => "v0.0");

            app.MapGet("/api/servers", (WorkerService worker) =>
                new PublicServerListResponse
                {
                    Servers = worker.PublicServers.Select(server => new PublicServerListing
                    {
                        Name = server.ServerName,
                        CurrentPlayers = server.CurrentPlayers,
                        MaxPlayers = server.MaxPlayers,
                        HasPassword = server.HasPassword,
                        Address = $"{server.ExternalAddress.Address}:{server.ExternalAddress.Port}",
                        ServerToken = server.ServerToken,
                    }).ToArray()
                });

            app.Run();
        }
    }
}

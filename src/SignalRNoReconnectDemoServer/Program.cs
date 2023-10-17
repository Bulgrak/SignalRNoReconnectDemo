using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SignalRNoReconnectDemoServer;

internal class Program
{
    private static void Main(string[] args)
    {
        var configuration = BuildConfiguration();
        IHost host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new DryIocServiceProviderFactory())
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(GetUrlsFromConfiguration(configuration));
                    webBuilder.UseKestrel();
                })
                .UseNLog()
                .Build();
                
        host.Run();
    }

    private static string[] GetUrlsFromConfiguration(IConfiguration configuration)
    {
        var urls = configuration["ServerConfig:Urls"];
        return urls.Split(',');
    }

    public static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
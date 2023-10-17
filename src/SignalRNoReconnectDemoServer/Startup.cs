using Signalr.Server;
using Signalr.Server.Interfaces;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using NLog.Extensions.Logging;

namespace SignalRNoReconnectDemoServer
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme).AddCertificate();
            services.AddSignalR(o =>
            {
                var config = Configuration.GetSection(nameof(ServerConfig)).Get<ServerConfig>();
                o.EnableDetailedErrors = true;
                o.MaximumParallelInvocationsPerClient = 10;
                o.MaximumReceiveMessageSize = config.MaximumReceiveMessageSize;
            })
            .AddMessagePackProtocol();
            services.AddLogging(x =>
            {
                x.AddNLog();
            });
            //services.AddTransient<ILogger>(provider =>
            //{
            //    // Hack to register ILogger as a dependency
            //    // See the following for more info:
            //    // - https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-GetCurrentClassLogger-and-Microsoft-ILogger
            //    // - https://stackoverflow.com/questions/52921966/unable-to-resolve-ilogger-from-microsoft-extensions-logging
            //    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            //    const string categoryName = "Any";
            //    return loggerFactory.CreateLogger(categoryName);
            //});
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddSingleton<ISignalrServerController, ServerController>();
            services.AddHostedService<Worker>();

            services.AddOptions();

            services.Configure<ServerConfig>(Configuration.GetSection(typeof(ServerConfig).Name));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ServerHub>("/hubs/server");
            });
        }
    }
}
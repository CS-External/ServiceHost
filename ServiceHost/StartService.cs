using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceHost.Logging;
using ServiceHost.Managers;
using ServiceHost.Utils;

namespace ServiceHost
{
    public static class StartService
    {

        public static async Task Run(string[] p_Args)
        {
            Console.WriteLine("Starting ServiceHost");

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Util.CreateDefaultConfigFile();

            IHostBuilder l_Builder = Host.CreateDefaultBuilder(p_Args);
            l_Builder.ConfigureServices((p_Context, p_Services) =>
            {
                p_Services.AddHostedService<Worker>();
                p_Services.AddSingleton<FileManager>();
                p_Services.AddSingleton<UpdateManager>();
                p_Services.AddSingleton<ProcessManager>();
                p_Services.Configure<ProcessOptions>(p_Context.Configuration.GetSection(nameof(ProcessOptions)));
            }).ConfigureLogging((p_Context, p_LoggingBuilder) =>
            {
                p_LoggingBuilder.AddFile("Logs/{Date}.txt", fileSizeLimitBytes: 100 * 1024 * 1024); // 100mb
                p_LoggingBuilder.ConnectToGrafanaLoki(p_Context.Configuration.GetSection("Logging:GrafanaLoki"), p_Context.Configuration.GetSection("ServiceOptions:ServiceName").Value, p_Context.Configuration.GetSection("ProcessOptions:App").Value);
            });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                l_Builder.UseWindowsService();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                l_Builder.UseSystemd();
            }

            IHost l_Host = l_Builder.Build();


            await l_Host.RunAsync();
        }

    }
}

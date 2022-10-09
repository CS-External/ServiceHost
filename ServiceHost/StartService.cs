using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceHost.Managers;
using ServiceHost.Utils;

namespace ServiceHost
{
    public static class StartService
    {

        public static async Task Run(string[] p_Args)
        {
            Console.WriteLine("Starting ServiceHost");

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

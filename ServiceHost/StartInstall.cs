using ServiceHost.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ServiceHost
{
    public static class StartInstall
    {
        public static async Task Run(string[] p_Args)
        {
            
            String l_ServiceName = Util.ReadServiceNameFromConfig();

            if (String.IsNullOrWhiteSpace(l_ServiceName))
            {
                Console.Error.WriteLine("No ServiceName defined. Please define ServiceOptions.ServiceName in appsettings.json");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await RunWindows(l_ServiceName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await RunLinux(l_ServiceName);
            }
            else
            {
                Console.WriteLine("Current Platform not supported");
            }


        }

        private static async Task RunLinux(string p_ServiceName)
        {
            string l_Path = $"/etc/systemd/system/{p_ServiceName}.service";

            if (File.Exists(l_Path))
            {
                Console.WriteLine($"Service {p_ServiceName} already installed");
                return;
            }

            string l_PathToExecutable = Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name);

            Console.WriteLine($"Create Service File {l_Path}");

            await File.WriteAllTextAsync(l_Path, "[Unit]\r\n" +
                                      $"Description={p_ServiceName}\r\n" +
                                      "[Service]\r\n" +
                                      "Type=notify\r\n" +
                                      $"WorkingDirectory={Directory.GetCurrentDirectory()}\r\n" +
                                      $"ExecStart={l_PathToExecutable}\r\n" +
                                      "[Install]\r\n" +
                                      "WantedBy=multi-user.target");

            Console.WriteLine("Reload Service");
            await Util.RunExternalProcess("systemctl", "daemon-reload");

        }

        private static async Task RunWindows(string p_ServiceName)
        {
            string l_Path = Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name);
            string l_FinalPath = Path.ChangeExtension(l_Path, ".exe");
            l_FinalPath = "\\\"" + l_FinalPath + "\\\" servicedir \\\"" + System.IO.Directory.GetCurrentDirectory() + "\\\"";
            await Util.RunExternalProcess("sc", $"create {Util.Quote(p_ServiceName)} BinPath= {Util.Quote(l_FinalPath)} start= auto");

        }
    }
}

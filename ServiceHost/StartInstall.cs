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
            

        }

        private static async Task RunWindows(string p_ServiceName)
        {
            string l_Path = Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name);
            string l_FinalPath = Path.ChangeExtension(l_Path, ".exe");
            l_FinalPath = "\\\"" + l_FinalPath + "\\\" servicedir \\\"" + System.IO.Directory.GetCurrentDirectory() + "\\\"";
            await Util.RunExternalProcess("sc", $"create {Util.Quote(p_ServiceName)} BinPath= {Util.Quote(l_FinalPath)}");

        }
    }
}

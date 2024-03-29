﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ServiceHost.Utils;

namespace ServiceHost
{
    public static class StartUninstall
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

            if (!File.Exists(l_Path))
            {
                return;
            }

            Console.WriteLine($"Delete Service File {l_Path}");
            File.Delete(l_Path);

            Console.WriteLine("Reload Service");
            await Util.RunExternalProcess("systemctl", "daemon-reload");

        }

        private static async Task RunWindows(string p_ServiceName)
        {
            await Util.RunExternalProcess("sc", $"delete {Util.Quote(p_ServiceName)}");

        }
    }
}

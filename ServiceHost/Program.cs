using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceHost;
using ServiceHost.Managers;
using System.Runtime.InteropServices;
using ServiceHost.Utils;


if (args.Length == 0)
{
    await StartService.Run(args);
}
else
if (args[0].ToLower() == "servicedir")
{
    System.IO.Directory.SetCurrentDirectory(args[1]);
    await StartService.Run(args);
}
else
if (args[0].ToLower() == "install")
{
    await StartInstall.Run(args);
}
else
if (args[0].ToLower() == "uninstall")
{
    await StartUninstall.Run(args);
}
else
{
    await StartHelp.Run();
}
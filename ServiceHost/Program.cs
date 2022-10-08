using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceHost;
using ServiceHost.Managers;

IHostBuilder l_Builder = Host.CreateDefaultBuilder(args);
l_Builder.ConfigureServices((p_Context, p_Services) =>
{
    p_Services.AddHostedService<Worker>();
    p_Services.AddSingleton<FileManager>();
    p_Services.AddSingleton<UpdateManager>();
    p_Services.AddSingleton<ProcessManager>();
    p_Services.Configure<ProcessOptions>(p_Context.Configuration.GetSection(nameof(ProcessOptions)));
});

IHost l_Host = l_Builder.Build();


await l_Host.RunAsync();
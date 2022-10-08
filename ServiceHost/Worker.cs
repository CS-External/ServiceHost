using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost.Managers;

namespace ServiceHost;

public class Worker : BackgroundService
{
    private readonly IOptionsMonitor<ProcessOptions> m_Options;
    private readonly ILogger<Worker> m_Logger;
    private readonly FileManager m_FileManager;
    private readonly UpdateManager m_UpdateManager;
    private readonly ProcessManager m_ProcessManager;

    public Worker(ILogger<Worker> p_Logger, IOptionsMonitor<ProcessOptions> p_Options, UpdateManager p_UpdateManager,
        FileManager p_FileManager, ProcessManager p_ProcessManager)
    {
        m_Logger = p_Logger;
        m_Options = p_Options;
        m_FileManager = p_FileManager;
        m_UpdateManager = p_UpdateManager;
        m_ProcessManager = p_ProcessManager;
    }

    protected override async Task ExecuteAsync(CancellationToken p_StoppingToken)
    {
        while (!p_StoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoExecute(p_StoppingToken);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;
                
                m_Logger.LogCritical(e, "Unhandled Error occurred. Restart Application in 10 Sec");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }    
        }
    }

    private async Task DoExecute(CancellationToken p_StoppingToken)
    {
        using (IDisposable l_Change = m_Options.OnChange((p_NewOptions) =>
               {
                   
                   m_Logger.LogInformation("Options changed to {p_NewOptions}", p_NewOptions);
                   
               }))
        {
            await m_FileManager.Setup();
            await m_ProcessManager.Setup();
            await m_UpdateManager.StartCheckForUpdates();

            while (!p_StoppingToken.IsCancellationRequested)
            {
                await m_UpdateManager.ExecuteUpdate();

                await m_ProcessManager.Start();

                while (m_ProcessManager.IsRunning)
                {
                    if (p_StoppingToken.IsCancellationRequested || m_UpdateManager.UpdateAvailable)
                    {
                        await m_ProcessManager.Stop();
                    }

                    await Task.Delay(5000, p_StoppingToken);
                }
                
                if (!p_StoppingToken.IsCancellationRequested)
                    await Task.Delay(5000, p_StoppingToken);
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceHost.Managers;

public class ProcessManager
{
    private readonly FileManager m_FileManager;
    private readonly ILogger<ProcessManager> m_Logger;
    private Process m_Process;
    private IDisposable m_ChildProcess;
    private readonly IOptionsMonitor<ProcessOptions> m_Options;

    public ProcessManager(FileManager p_FileManager, IOptionsMonitor<ProcessOptions> p_Options, ILogger<ProcessManager> p_Logger)
    {
        m_FileManager = p_FileManager;
        m_Options = p_Options;
        m_Logger = p_Logger;
    }

    public bool IsRunning
    {
        get
        {
            Process l_Process = m_Process;

            if (l_Process == null)
                return false;

            return !l_Process.HasExited;
        }
    }

    public async Task Start()
    {

        if (m_Process != null)
        {
            await Stop();
            ResetProcess();
        }
        
        ProcessOptions l_Options = m_Options.CurrentValue;

        if (String.IsNullOrWhiteSpace(l_Options.App))
        {
            m_Logger.LogWarning("No Application defined for launching");  
            return;
        }

        string l_StartInfoFileName = Path.Combine(m_FileManager.AppPath, l_Options.App);

        if (!File.Exists(l_StartInfoFileName))
        {
            m_Logger.LogWarning("Application {p_App} not exists", l_StartInfoFileName);  
            return;
        }
        
        m_Logger.LogInformation("Start Application {p_App}", l_Options.App);
        
        ProcessStartInfo l_StartInfo = new ProcessStartInfo();
        l_StartInfo.FileName = l_StartInfoFileName;

        if (l_Options.UseCustomWorkDirectory)
        {
            l_StartInfo.WorkingDirectory = m_FileManager.WorkDirPath;
        }
        else
        {
            l_StartInfo.WorkingDirectory = m_FileManager.AppPath;
        }

        if (!String.IsNullOrWhiteSpace(l_Options.AppArguments))
            l_StartInfo.Arguments = l_Options.AppArguments;

        if (l_Options.CheckForUpdates)
        {
            l_StartInfo.EnvironmentVariables.Add("ServiceHostUpdatePath", m_FileManager.UpdatePath);
        }

        ApplyDefaultSettings(l_StartInfo);
        m_Process = Process.Start(l_StartInfo);

        if (m_Process == null)
            throw new Exception($"Unable to Start Application {l_StartInfo.FileName}");

        AttachToProcessTree();
        m_Process.OutputDataReceived += ProcessOnOutputDataReceived;
        m_Process.Exited += ProcessOnExited;
        m_Process.BeginOutputReadLine();
        m_Process.BeginErrorReadLine();
    }

    private void ProcessOnExited(object p_Sender, EventArgs p_Args)
    {
        Process l_Process = (Process)p_Sender;

        if (l_Process.ExitCode == 0)
        {
            m_Logger.LogInformation("Application stopped");     
        }
        else
        {
            m_Logger.LogWarning("Application stopped unexpected. (ExitCode {p_ExitCode})", l_Process.ExitCode);     
        }
        
    }

    private void AttachToProcessTree()
    {
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ChildProcessWindows l_Windows = new ChildProcessWindows();
            l_Windows.AddProcess(m_Process);
            m_ChildProcess = l_Windows;
        }
    }

    private void ProcessOnOutputDataReceived(object p_Sender, DataReceivedEventArgs p_Args)
    {
        if (String.IsNullOrWhiteSpace(p_Args.Data))
            return;
        
        m_Logger.LogInformation(p_Args.Data);
        
    }

    private void ApplyDefaultSettings(ProcessStartInfo p_StartInfo)
    {
        p_StartInfo.RedirectStandardInput = true;
        p_StartInfo.RedirectStandardError = true;
        p_StartInfo.RedirectStandardOutput = true;
        p_StartInfo.CreateNoWindow = true;
        p_StartInfo.UseShellExecute = false;
    }

    public async Task Stop()
    {
        
        if (m_Process == null)
            return;

        if (!m_Process.HasExited)
        {
            m_Logger.LogInformation("Shutdown Application");
            
            m_Process.CloseMainWindow();

            if (m_Process.HasExited)
                return;
            
            try
            {
                await m_Process.WaitForExitAsync(new CancellationTokenSource(1000).Token);
            }
            catch (Exception e)
            {
                
                if (!(e is TaskCanceledException))
                    throw;
            }

            if (m_Process.HasExited)
            {
                return;
            }
            
            m_Logger.LogInformation("Shutdown Application not successful. Kill Application");
            
            m_Process.Kill(true); 
            await m_Process.WaitForExitAsync();
            
            
        }
        
    }

    public async Task Setup()
    {
        if (m_Process == null)
            return;
        
        await Stop();
        ResetProcess();
    }

    private void ResetProcess()
    {
        m_ChildProcess?.Dispose();
        m_ChildProcess = null;

        if (m_Process != null)
        {
            m_Process.OutputDataReceived -= ProcessOnOutputDataReceived;
            m_Process.Exited -= ProcessOnExited;
            m_Process.Dispose();
        }
        
        m_Process = null;
    }
}
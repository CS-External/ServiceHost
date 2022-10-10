using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceHost.Managers;

public class UpdateManager: IDisposable
{
    private readonly FileManager m_FileManager;
    private readonly ILogger<UpdateManager> m_Logger;
    private FileSystemWatcher m_Watcher;
    private Boolean m_UpdateAvailable;
    private readonly IOptionsMonitor<ProcessOptions> m_Options;
   
    public UpdateManager(FileManager p_FileManager, ILogger<UpdateManager> p_Logger, IOptionsMonitor<ProcessOptions> p_Options)
    {
        m_FileManager = p_FileManager;
        m_Logger = p_Logger;
        m_Options = p_Options;
    }

    public bool UpdateAvailable
    {
        get
        {
            return m_UpdateAvailable;
        }
    }

    public async Task ExecuteUpdate()
    {
        if (!m_UpdateAvailable)
            return;

        try
        {
            if (!m_Options.CurrentValue.CheckForUpdates)
            {
                m_Logger.LogInformation("Update Available but Option For Update Check ist not active");
                return;
            }
                
            
            UpdateMode l_UpdateMode = CalcUpdateMode();
        
            if (l_UpdateMode == UpdateMode.None)
                return;
            
            m_Logger.LogInformation("Start Update Application. (Mode {l_UpdateMode})", l_UpdateMode);
            
            m_Logger.LogInformation("Copy Current Application to Backup Location");
            await m_FileManager.CopyApplicationToBackup();
            try
            {
                switch (l_UpdateMode)
                {
                    case UpdateMode.Zip:
                        m_Logger.LogInformation("Extract new Version to Application Location");
                        await ExtractUpdateToApplication();  
                        break;
                    case UpdateMode.Directory:
                        m_Logger.LogInformation("Copy new Version to Application Location");
                        await CopyUpdateToApplication();    
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                m_Logger.LogInformation("Update Finished");
            }
            catch (Exception e)
            {
                m_Logger.LogError(e, "Update Failed. Try to Restore Old Version");
                await RestoreOldVersion();
            }
            
            
        }
        finally
        {
            await m_FileManager.CleanUpUpdate();
            m_UpdateAvailable = false;    
        }
        
    }

    private async Task RestoreOldVersion()
    {
        try
        {
            await m_FileManager.CopyBackupToApplication(); 
            m_Logger.LogInformation("Restore Old Version successful");
        }
        catch (Exception e)
        {
            m_Logger.LogError(e, "Restore Old Version Failed");       
        }
          
    }

    private async Task ExtractUpdateToApplication()
    {
        await m_FileManager.CleanUpApp(false);
        
        string l_ZipFile = Directory.EnumerateFiles(m_FileManager.UpdatePath).First();

        using (FileStream l_Stream = new FileStream(l_ZipFile, FileMode.Open, FileAccess.Read))
        {
            using (ZipArchive l_ZipArchive = new ZipArchive(l_Stream, ZipArchiveMode.Read))
            {
                l_ZipArchive.ExtractToDirectory(m_FileManager.AppPath);
            }       
            
        }
    }

    private UpdateMode CalcUpdateMode()
    {
        string[] l_Directories = Directory.GetDirectories(m_FileManager.UpdatePath);
        string[] l_Files = Directory.GetFiles(m_FileManager.UpdatePath);

        if (l_Directories.Any() || l_Files.Any())
        {
            if (l_Directories.Any())
                return UpdateMode.Directory;

            if (l_Files.Length > 1)
                return UpdateMode.Directory;


            string l_Extension = Path.GetExtension(l_Files[0]);
            if (!String.IsNullOrWhiteSpace(l_Extension) && l_Extension.ToLower() == ".zip")
                return UpdateMode.Zip;

            return UpdateMode.Directory;
        }
        else
        {
            return UpdateMode.None;
        }
        
        
    }

    private async Task CopyUpdateToApplication()
    {
        await m_FileManager.CleanUpApp(false);  
        await Utils.Util.CopyFilesRecursively(m_FileManager.UpdatePath, m_FileManager.AppPath);
    }

    public Task StartCheckForUpdates()
    {
        if (m_Watcher != null)
        {
            m_Watcher.Dispose();
            m_Watcher = null;
        }
        
        Action<object,FileSystemEventArgs> l_Debounce = Utils.Util.Debounce<Object, FileSystemEventArgs>(WatcherOnChanged, TimeSpan.FromSeconds(30));
        FileSystemEventHandler l_Handler = new FileSystemEventHandler((p_Sender, p_Args) =>
        {
            m_Logger.LogInformation("Update found. Wait for stable results");
            l_Debounce(p_Sender, p_Args);
        });
        
        m_Watcher = new FileSystemWatcher(m_FileManager.UpdatePath);
        m_Watcher.Created += l_Handler;
        m_Watcher.Changed += l_Handler;
        m_Watcher.Error += WatcherOnError;
        m_Watcher.IncludeSubdirectories = true;
        m_Watcher.EnableRaisingEvents = true;

        // Check if Update is already available 
        if (Directory.EnumerateDirectories(m_FileManager.UpdatePath).Any() ||
            Directory.EnumerateFiles(m_FileManager.UpdatePath).Any())
        {
            m_Logger.LogInformation("Stable Update found");
            m_UpdateAvailable = true;
        }
        else
        {
            m_UpdateAvailable = false;
        }
        
        return Task.CompletedTask;
        
    }

    private void WatcherOnError(object p_Sender, ErrorEventArgs p_Args)
    {
        Exception l_Exception = p_Args.GetException();
        m_Logger.LogError(l_Exception, "Error while Watcher for Updates");
    }

    private void WatcherOnChanged(object p_Sender, FileSystemEventArgs p_Args)
    {
        m_Logger.LogInformation("Update found");
        m_UpdateAvailable = true;
    }

    public void Dispose()
    {
        m_Watcher?.Dispose();
        m_Watcher = null;
    }
    
}
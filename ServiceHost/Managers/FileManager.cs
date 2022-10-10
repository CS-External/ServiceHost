using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost.Utils;

namespace ServiceHost.Managers;

public class FileManager
{
    private readonly ILogger<FileManager> m_Logger;
    private readonly IOptionsMonitor<ProcessOptions> m_Options;

    public String AppPath { get; private set; }
    private String AppBackupPath { get; set; }
    public String UpdatePath { get; private set; }
    public String WorkDirPath { get; private set; }

    public FileManager(ILogger<FileManager> p_Logger, IOptionsMonitor<ProcessOptions> p_Options)
    {
        m_Logger = p_Logger;
        m_Options = p_Options;
    }

    public Task Setup()
    {
        m_Logger.LogInformation("Start FileManager");
        
        AppPath = Path.Combine(Environment.CurrentDirectory, "App");
        AppBackupPath = Path.Combine(Environment.CurrentDirectory, "AppBackup");
        UpdatePath = Path.Combine(Environment.CurrentDirectory, "Update");
        WorkDirPath = Path.Combine(Environment.CurrentDirectory, "WorkDir");

        EnsureDirectoryExists(AppPath);
        EnsureDirectoryExists(UpdatePath);
        EnsureDirectoryExists(WorkDirPath);
        EnsureDirectoryExists(AppBackupPath);
        return Task.CompletedTask;
        

    }

    private void EnsureDirectoryExists(string p_Path)
    {
        if (p_Path == null)
            throw new ArgumentNullException(nameof(p_Path));
        
        if (!Directory.Exists(p_Path))
            Directory.CreateDirectory(p_Path);    
    }
    
    public async Task CleanUpApp(bool p_DeleteAll)
    {
        if (p_DeleteAll)
        {
            await Util.DeleteDirectoryContent(AppPath, null);
        }
        else
        {
            ProcessOptions l_Options = m_Options.CurrentValue;

            if (l_Options.FilesToKeepOnUpdate == null || l_Options.FilesToKeepOnUpdate.Count == 0)
            {
                await Util.DeleteDirectoryContent(AppPath, null);
            }

            await Util.DeleteDirectoryContent(AppPath, (p_Info) =>
            {
                string l_CurrentDirectory = AppPath;

                foreach (string l_Item in l_Options.FilesToKeepOnUpdate)
                {
                    if (String.IsNullOrWhiteSpace(l_Item))
                        continue;

                    string l_CompletePath = Path.Combine(l_CurrentDirectory, l_Item);

                    if (l_Item.EndsWith(Path.DirectorySeparatorChar))
                    {
                        // Its a Dir
                        if (p_Info is DirectoryInfo)
                        {
                            string l_Name = p_Info.FullName;

                            if (!l_Name.EndsWith(Path.DirectorySeparatorChar))
                                l_Name = l_Name + Path.DirectorySeparatorChar;

                            if (!l_CompletePath.EndsWith(Path.DirectorySeparatorChar))
                                l_CompletePath = l_CurrentDirectory + Path.DirectorySeparatorChar;

                            if (l_CompletePath == l_Name)
                                return false;
                        }

                    }
                    else
                    {
                        // Its a file
                        if (p_Info is FileInfo)
                        {

                            if (p_Info.FullName == l_CompletePath)
                                return false;

                        }
                    }

                }

                return true;


            });
        }

        
        
    }

    public async Task CopyApplicationToBackup()
    {
        ProcessOptions l_Options = m_Options.CurrentValue;

        await CleanUpAppBackup();
        EnsureDirectoryExists(AppPath);
        await Util.CopyFilesRecursively(AppPath, AppBackupPath); 
        
    }
    
    private async Task CleanUpAppBackup()
    {
        await Util.DeleteDirectoryContent(AppBackupPath, null);
    }

    public async Task CleanUpUpdate()
    {
        await Util.DeleteDirectoryContent(UpdatePath, null);
    }

    public async Task CopyBackupToApplication()
    {
        await CleanUpApp(true);
        EnsureDirectoryExists(AppBackupPath);
        await Util.CopyFilesRecursively(AppBackupPath, AppPath);     
    }
}
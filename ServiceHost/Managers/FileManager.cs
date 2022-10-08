﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceHost.Utils;

namespace ServiceHost.Managers;

public class FileManager
{
    private readonly ILogger<FileManager> m_Logger;

    public String AppPath { get; private set; }
    private String AppBackupPath { get; set; }
    public String UpdatePath { get; private set; }
    public String WorkDirPath { get; private set; }

    public FileManager(ILogger<FileManager> p_Logger)
    {
        m_Logger = p_Logger;
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
    
    public async Task CleanUpApp()
    {
        await Util.DeleteDirectoryContent(AppPath);
    }

    public async Task CopyApplicationToBackup()
    {
        await CleanUpAppBackup();
        EnsureDirectoryExists(AppPath);
        await Util.CopyFilesRecursively(AppPath, AppBackupPath); 
        
    }
    
    private async Task CleanUpAppBackup()
    {
        await Util.DeleteDirectoryContent(AppBackupPath);
    }

    public async Task CleanUpUpdate()
    {
        await Util.DeleteDirectoryContent(UpdatePath);
    }

    public async Task CopyBackupToApplication()
    {
        await CleanUpApp();
        EnsureDirectoryExists(AppBackupPath);
        await Util.CopyFilesRecursively(AppBackupPath, AppPath);     
    }
}
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceHost.Utils;

public static class Util
{
    public static String Quote(String p_Text)
    {
        return "\"" + p_Text + "\"";
    }

    public static async Task RunExternalProcess(String p_Cmd, String p_Args)
    {
        Console.WriteLine($"Run {p_Cmd} {p_Args}");
        Console.WriteLine("");

        ProcessStartInfo l_StartInfo = new ProcessStartInfo();
        l_StartInfo.FileName = p_Cmd;

        if (!String.IsNullOrWhiteSpace(p_Args))
            l_StartInfo.Arguments = p_Args;

        l_StartInfo.RedirectStandardInput = true;
        l_StartInfo.RedirectStandardError = true;
        l_StartInfo.RedirectStandardOutput = true;
        l_StartInfo.CreateNoWindow = true;
        l_StartInfo.UseShellExecute = false;

        Process l_Process = Process.Start(l_StartInfo);
        try
        {
            l_Process.OutputDataReceived += ProcessOnOutputDataReceived;
            l_Process.BeginOutputReadLine();
            l_Process.BeginErrorReadLine();

            await l_Process.WaitForExitAsync();
        }
        finally
        {
            l_Process.OutputDataReceived -= ProcessOnOutputDataReceived;
            l_Process.Dispose();
        }
        
    }

    private static void ProcessOnOutputDataReceived(object p_Sender, DataReceivedEventArgs p_Args)
    {
        if (String.IsNullOrWhiteSpace(p_Args.Data))
            return;

        Console.WriteLine(p_Args.Data);
    }

    public static async Task DeleteDirectoryContent(String p_Path)
    {
        if (Directory.Exists(p_Path))
        {
            int l_Count = 0;
            
            while (true)
            {
                l_Count++;
                try
                {
                    string[] l_Directories = Directory.GetDirectories(p_Path);

                    foreach (string l_Directory in l_Directories)
                    {
                        Directory.Delete(Path.Combine(p_Path, l_Directory), true);    
                    }

                    string[] l_Files = Directory.GetFiles(p_Path);

                    foreach (string l_File in l_Files)
                    {
                        File.Delete(Path.Combine(p_Path, l_File));
                    }


                    break;
                }
                catch (Exception)
                {
                    if (l_Count < 3)
                        await Task.Delay(1000);
                    else
                        throw;
                    
                }
            }
            
            
            
            
        }
    }
    
    
    public static Action<T1, T2> Debounce<T1, T2>(this Action<T1, T2> p_Func, TimeSpan p_Time)
    {
        CancellationTokenSource l_CancelTokenSource = null;

        return (p_Arg1, p_Arg2) =>
        {
            l_CancelTokenSource?.Cancel();
            l_CancelTokenSource = new CancellationTokenSource();

            Task.Delay(p_Time, l_CancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        p_Func(p_Arg1, p_Arg2);
                    }
                }, TaskScheduler.Default);
        };
    }
    
    
    public static async Task CopyFilesRecursively(string p_SourcePath, string p_TargetPath)
    {
        await Task.Run(() =>
        {
            //Now Create all of the directories
            foreach (string l_DirPath in Directory.GetDirectories(p_SourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(l_DirPath.Replace(p_SourcePath, p_TargetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string l_NewPath in Directory.GetFiles(p_SourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(l_NewPath, l_NewPath.Replace(p_SourcePath, p_TargetPath), true);
            }
        });
    }

    public static void CreateDefaultConfigFile()
    {
        if (File.Exists("appsettings.json"))
            return;

        Console.WriteLine("Create default appsettings.json");

        File.WriteAllText("appsettings.json", "{\r\n  \"Logging\": {\r\n    \"LogLevel\": {\r\n      \"Default\": \"Information\",\r\n      \"Microsoft.Hosting.Lifetime\": \"Information\"\r\n    }\r\n  },\r\n  \"ProcessOptions\": {\r\n    \"App\": \"\", // Name of the Exectable to start \r\n    \"AppArguments\": \"\", // Arguments for the Exectable\r\n    \"UseCustomWorkDirectory\": false, // Exectable has different working Directory\r\n    \"CheckForUpdates\": false, // Check for new Versions in the Update Directory\r\n    \"FilesToKeepOnUpdate\": [] // Files which are not deleted durring update\r\n  },\r\n  \"ServiceOptions\": {\r\n    \"ServiceName\":  \"\" // Name of the Service which use by the install command\r\n  }\r\n}");
            
    }

    public static string ReadServiceNameFromConfig()
    {

        if (!File.Exists("appsettings.json"))
            return "";

        ConfigurationBuilder l_Builder = new ConfigurationBuilder();
        l_Builder.SetBasePath(Directory.GetCurrentDirectory());
        l_Builder.AddJsonFile("appsettings.json");
        IConfigurationRoot l_Root = l_Builder.Build();

        IConfigurationSection l_ConfigurationSection = l_Root.GetSection(nameof(ServiceOptions));

        if (!l_ConfigurationSection.Exists())
            return string.Empty;

        return l_ConfigurationSection.GetValue<String>(nameof(ServiceOptions.ServiceName));
    }
}
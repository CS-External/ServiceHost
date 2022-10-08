using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceHost.Utils;

public static class Util
{

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
}
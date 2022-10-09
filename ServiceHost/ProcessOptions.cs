using System;
using System.Collections.Generic;

namespace ServiceHost;

public class ProcessOptions
{
    public String App { get; set; }
    public String AppArguments { get; set; }
    public Boolean UseCustomWorkDirectory { get; set; }
    public Boolean CheckForUpdates { get; set; }
    public List<String> FilesToKeepOnUpdate { get; set; }

    public override string ToString()
    {
        return $"{nameof(App)}: {App}, {nameof(UseCustomWorkDirectory)}: {UseCustomWorkDirectory}, {nameof(CheckForUpdates)}: {CheckForUpdates}, {nameof(FilesToKeepOnUpdate)}: {FilesToKeepOnUpdate}";
    }
}
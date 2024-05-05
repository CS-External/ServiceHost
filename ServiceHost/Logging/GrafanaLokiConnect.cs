using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace ServiceHost.Logging;

public static class GrafanaLokiConnect
{
    public static void ConnectToGrafanaLoki(this ILoggingBuilder p_LoggingBuilder,
        IConfigurationSection p_LoggingConfig, String p_ServiceName, String p_AppName)
    {
        if (p_LoggingConfig == null)
            return;

        string l_Url = p_LoggingConfig.GetSection("Url")?.Value;

        if (String.IsNullOrWhiteSpace(l_Url))
            return;
        
        string l_User = p_LoggingConfig.GetSection("Username")?.Value;
        string l_Password = p_LoggingConfig.GetSection("Password")?.Value;

        List<LokiLabel> l_Labels = new List<LokiLabel>();

        if (!String.IsNullOrWhiteSpace(p_ServiceName))
            l_Labels.Add(new LokiLabel() { Key = "service", Value = p_ServiceName });

        if (!String.IsNullOrWhiteSpace(p_AppName))
            l_Labels.Add(new LokiLabel() { Key = "app", Value = p_AppName });

        if (p_LoggingConfig.GetSection("Labels") != null)
        {
            foreach (var l_Label in p_LoggingConfig.GetSection("Labels").GetChildren())
            {
                if (String.IsNullOrWhiteSpace(l_Label.Key) || String.IsNullOrWhiteSpace(l_Label.Value))
                    continue;

                l_Labels.Add(new LokiLabel() { Key = l_Label.Key, Value = l_Label.Value });
            }
        }
        
        LokiCredentials l_LokiCredentials = null;

        if (!String.IsNullOrWhiteSpace(l_User) && !String.IsNullOrWhiteSpace(l_Password))
            l_LokiCredentials = new LokiCredentials() { Login = l_User, Password = l_Password };

        var l_Logger = new LoggerConfiguration()
            .WriteTo.GrafanaLoki(
                l_Url, l_Labels, null, l_LokiCredentials)
            .CreateLogger();
        
        p_LoggingBuilder.AddSerilog(l_Logger);
        
        Console.WriteLine("Logging to Grafana Loki (" + l_Url + ") enabled");
    }
}
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Common.Helpers
{
    public static class LoggingHelper
    {
        /// <summary>
        /// Sets Serilog as the logging provider.
        /// </summary>
        public static IHostBuilder UseSerilog(this IHostBuilder builder)
        {
            return builder.UseSerilog((context, services, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);
                config.WriteTo.Console(levelSwitch: ApplicationConstants.ConsoleLogLevel);
                config.Enrich.FromLogContext();
            });
        }

        /// <summary>
        /// Initializes logger file.
        /// File name format is => {ApplicationName}.{LogRollingDate}.log
        /// </summary>
        public static void InitializeLogger(string[] args)
        {
            // Build configuration for logger.
            var configRoot = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(ApplicationConstants.AppSettingsFileName, false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // Configure the logger with appsettings configuration.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configRoot)
                .WriteTo.Async(a => a.Console(levelSwitch: ApplicationConstants.ConsoleLogLevel))
                .Enrich.FromLogContext()
                .CreateBootstrapLogger();
        }

        /// <summary>
        /// Log some detailed information messages for start up of the application
        /// </summary>
        public static void LogStartupMessages(string[] args)
        {
            var argsText = args.Length switch
            {
                > 1 => $"with arguments '{string.Join(' ', args)}'",
                > 0 => $"with argument '{args[0]}'",
                _ => "without any arguments"
            };

            Log.Information($"{ApplicationConstants.ApplicationName} is starting {argsText}.");
            Log.Information("Application launched as a process. Process Id: {ProcessId}", ApplicationConstants.ProcessId);
            Log.Information("Base directory path: {BaseDirectory}", AppContext.BaseDirectory);
            Log.Information("Operating System Platform: {OsPlatform}", ApplicationConstants.OperatingSystem);

            LogJenkinsBuildInfo();
        }

        /// <summary>
        /// Log buildInfo.properties 
        /// </summary>
        private static void LogJenkinsBuildInfo()
        {
            string buildInfoPath = Path.Combine(ApplicationConstants.ExecutionPath, "BuildInfo.properties");

            if (!File.Exists(buildInfoPath))
            {
                Log.Warning("Couldn't find the BuildInfo.properties file.");
                return;
            }

            var buildInfo = File.ReadLines(buildInfoPath)
                .Where((l, i) => l?.Count(x => x == '=') == 1)
                .Select((l, i) =>
                {
                    var pair = l.Split("=");
                    return (pair[0], pair[1]);
                }).ToDictionary(k => k.Item1, v => v.Item2);

            buildInfo.Add("machineName", Environment.MachineName);
            buildInfo.Add("machineIP", GetLocalIPAddress());

            Log.Information("Build info of the application: {FraudApiBuildInfo}", buildInfo);
        }

        private static string GetLocalIPAddress()
        {
            string ip = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var adress in host.AddressList)
            {
                if (adress.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = adress.ToString();
                    break;
                }
            }
            return ip;
        }
    }
}
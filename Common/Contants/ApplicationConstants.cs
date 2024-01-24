using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Constants
{
    public static class ApplicationConstants
    {
        public static readonly string ApplicationName;

        public static readonly string EnvironmentName;

        public static readonly string OperatingSystem;

        public static readonly string ProcessId;

        public static readonly string AppSettingsFileName;

        public static readonly string HostingFileName;

        public static readonly string HostingOsFileName;

        public static readonly LoggingLevelSwitch ConsoleLogLevel;

        public static readonly string ExecutionPath;

        public static readonly List<string> AllowedDomainList;

        static ApplicationConstants()
        {
            // Initialize application name
            ApplicationName = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Name!;

            // Initialize the environment. It can be only Development or Staging or Production
            var environment = GetEnvironmentFromArguments() ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            EnvironmentName = environment is "Development" or "Staging" or "Production" ? environment : "Production";
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

            // Initialize execution directory path
            ExecutionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Initialize operating system name
            OperatingSystem = Environment.OSVersion.Platform.ToString();

            // Initialize ProcessId of the application
            ProcessId = Environment.ProcessId.ToString();

            // Initialize application settings file names
            AppSettingsFileName = "appsettings.json";
            HostingFileName = "hosting.json";
            HostingOsFileName = $"hosting.{OperatingSystem}.json";

            // Initialize console log level switch to be able to change log level in runtime
            ConsoleLogLevel = new LoggingLevelSwitch();

            AllowedDomainList = new List<string>() { "localhost", "mybestman" };
        }

        private static string GetEnvironmentFromArguments()
        {
            var arguments = Environment.GetCommandLineArgs();
            var environmentArgument = arguments.FirstOrDefault(arg => arg.StartsWith("--environment", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(environmentArgument))
                return null;

            if (environmentArgument.Contains('='))
            {
                var environment = environmentArgument.Split('=')[1].Trim();
                return string.IsNullOrWhiteSpace(environment) ? null : environment;
            }

            for (var i = 0; i < arguments.Length; ++i)
            {
                if (!arguments[i].Equals("--environment", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 < arguments.Length)
                    return arguments[i + 1].Trim();
            }

            return null;
        }
    }
}
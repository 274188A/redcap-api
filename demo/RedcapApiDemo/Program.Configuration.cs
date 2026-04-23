using System;
using System.IO;
using System.Text.Json;

namespace RedcapApiDemo
{
    partial class Program
    {
        static bool _nonInteractive;

        static void InitializeInteractiveMode(string[] args)
        {
            _nonInteractive = HasNonInteractiveFlag(args)
                || IsTruthyEnvVar("REDCAP_DEMO_NON_INTERACTIVE")
                || Console.IsInputRedirected;

            if(_nonInteractive)
            {
                Console.WriteLine("[non-interactive mode] prompts will auto-accept defaults and 'Press Enter' pauses will be skipped.");
            }
        }

        static bool HasNonInteractiveFlag(string[] args)
        {
            if(args == null)
            {
                return false;
            }

            foreach(var arg in args)
            {
                if(string.Equals(arg, "--non-interactive", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(arg, "-n", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsTruthyEnvVar(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if(string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();
            return value.Equals("1", StringComparison.Ordinal)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        static string ReadInput()
        {
            if(_nonInteractive)
            {
                return string.Empty;
            }

            return Console.ReadLine();
        }

        static void WaitForEnter()
        {
            if(_nonInteractive)
            {
                return;
            }

            Console.ReadLine();
        }

        sealed class DevelopmentSettings
        {
            public string BaseUri { get; set; }
            public string ProjectToken { get; set; }
            public string SuperToken { get; set; }
            public string DownloadPath { get; set; }
        }

        sealed class DevelopmentSettingsRoot
        {
            public DevelopmentSettings RedcapDemo { get; set; }
        }

        static DevelopmentSettings LoadDevelopmentSettings()
        {
            var settings = new DevelopmentSettings();
            var filePath = FindDevelopmentSettingsFile();
            if(!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var parsed = JsonSerializer.Deserialize<DevelopmentSettingsRoot>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if(parsed?.RedcapDemo != null)
                    {
                        settings = parsed.RedcapDemo;
                    }
                }
                catch (Exception ex)
                {
                    // Keep defaults but surface the parse failure so the user knows their config was ignored.
                    Console.WriteLine($"Warning: could not read {filePath} — falling back to defaults. ({ex.GetType().Name}: {ex.Message})");
                }
            }

            ApplyEnvironmentOverrides(settings);

            return settings;
        }

        static void ApplyEnvironmentOverrides(DevelopmentSettings settings)
        {
            if(settings == null)
            {
                return;
            }

            var baseUri = FirstNonEmptyEnvironmentVariable("REDCAP_DEMO_BASE_URI", "RedcapDemo__BaseUri");
            if(!string.IsNullOrWhiteSpace(baseUri))
            {
                settings.BaseUri = baseUri;
            }

            var projectToken = FirstNonEmptyEnvironmentVariable("REDCAP_DEMO_PROJECT_TOKEN", "RedcapDemo__ProjectToken");
            if(!string.IsNullOrWhiteSpace(projectToken))
            {
                settings.ProjectToken = projectToken;
            }

            var superToken = FirstNonEmptyEnvironmentVariable("REDCAP_DEMO_SUPER_TOKEN", "RedcapDemo__SuperToken");
            if(!string.IsNullOrWhiteSpace(superToken))
            {
                settings.SuperToken = superToken;
            }

            var downloadPath = FirstNonEmptyEnvironmentVariable("REDCAP_DEMO_DOWNLOAD_PATH", "RedcapDemo__DownloadPath");
            if(!string.IsNullOrWhiteSpace(downloadPath))
            {
                settings.DownloadPath = downloadPath;
            }
        }

        static string ResolveDownloadPath(DevelopmentSettings settings)
        {
            if(settings != null && !string.IsNullOrWhiteSpace(settings.DownloadPath))
            {
                return Environment.ExpandEnvironmentVariables(settings.DownloadPath.Trim());
            }

            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if(string.IsNullOrWhiteSpace(profile))
            {
                profile = Path.GetTempPath();
            }

            return Path.Combine(profile, "redcap_download_files");
        }

        static string FirstNonEmptyEnvironmentVariable(params string[] names)
        {
            foreach(var name in names)
            {
                var value = Environment.GetEnvironmentVariable(name);
                if(!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        static string FindDevelopmentSettingsFile()
        {
            var current = new DirectoryInfo(Environment.CurrentDirectory);
            while(current != null)
            {
                var candidate = Path.Combine(current.FullName, "appsettings.Development.json");
                if(File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}

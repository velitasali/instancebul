using System.Diagnostics;
using System.Text;

namespace Instancebul;

static class UwpAppFinder
{
    private static readonly Dictionary<string, string> _cache = new();

    public static string? FindUwpApp(string searchTerm)
    {
        // Check cache first
        var cacheKey = searchTerm.ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        try
        {
            // Use PowerShell to find UWP apps and return a shell:AppsFolder URI.
            // Launching via AUMID is the correct approach — directly executing the exe
            // inside WindowsApps\ requires TrustedInstaller access and breaks on updates.
            var script = $@"
                Get-AppxPackage | Where-Object {{
                    $_.Name -like '*{searchTerm}*' -and $_.IsFramework -eq $false
                }} | Select-Object -First 1 | ForEach-Object {{
                    $pfn = $_.PackageFamilyName
                    $manifestPath = Join-Path $_.InstallLocation 'AppxManifest.xml'
                    if (Test-Path $manifestPath) {{
                        [xml]$manifest = Get-Content $manifestPath
                        $applications = $manifest.Package.Applications.Application
                        if ($applications -is [System.Array]) {{
                            $appId = $applications[0].Id
                        }} else {{
                            $appId = $applications.Id
                        }}
                        if ($appId) {{
                            Write-Output ""shell:AppsFolder\$pfn!$appId""
                        }}
                    }}
                }}
            ";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(output) || process.ExitCode != 0)
            {
                return null;
            }

            var result = output.Trim();

            _cache[cacheKey] = result;

            return result;
        }
        catch
        {
            return null;
        }
    }

    public static void ClearCache()
    {
        _cache.Clear();
    }
}

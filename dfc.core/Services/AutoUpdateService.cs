using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Handles checking for and installing application updates from GitHub releases
/// </summary>
public class AutoUpdateService : IAutoUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AutoUpdateService>? _logger;
    private const string GitHubApiUrl = "https://api.github.com/repos/{owner}/{repo}/releases/latest";
    private static readonly string CurrentVersion = GetCurrentVersion();

    public string Owner { get; set; } = "brobata";
    public string Repository { get; set; } = "Desktop Food CostAvalonia";
    public string? GitHubToken { get; set; }

    public AutoUpdateService(ILogger<AutoUpdateService>? logger = null, string? gitHubToken = null)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Desktop Food Cost-Desktop");

        // Add GitHub token for private repository access
        GitHubToken = gitHubToken;
        if (!string.IsNullOrWhiteSpace(GitHubToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {GitHubToken}");
        }

        _logger = logger;
    }

    /// <summary>
    /// Check if a new version is available on GitHub
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdateAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("========== AUTO-UPDATE CHECK STARTING ==========");
            System.Diagnostics.Debug.WriteLine($"Current Version: {CurrentVersion}");
            System.Diagnostics.Debug.WriteLine($"Repository: {Owner}/{Repository}");
            System.Diagnostics.Debug.WriteLine($"GitHub Token Present: {!string.IsNullOrWhiteSpace(GitHubToken)}");
            System.Diagnostics.Debug.WriteLine($"GitHub Token Value: {(string.IsNullOrWhiteSpace(GitHubToken) ? "NULL/EMPTY" : GitHubToken.Substring(0, Math.Min(10, GitHubToken.Length)) + "...")}");

            _logger?.LogInformation("========== AUTO-UPDATE CHECK STARTING ==========");
            _logger?.LogInformation("Current Version: {CurrentVersion}", CurrentVersion);
            _logger?.LogInformation("Repository: {Owner}/{Repo}", Owner, Repository);
            _logger?.LogInformation("GitHub Token Present: {HasToken}", !string.IsNullOrWhiteSpace(GitHubToken));

            var url = GitHubApiUrl
                .Replace("{owner}", Owner)
                .Replace("{repo}", Repository);

            System.Diagnostics.Debug.WriteLine($"GitHub API URL: {url}");
            _logger?.LogInformation("GitHub API URL: {Url}", url);

            System.Diagnostics.Debug.WriteLine("Sending HTTP request to GitHub...");
            _logger?.LogInformation("Sending HTTP request to GitHub...");
            var response = await _httpClient.GetAsync(url);
            System.Diagnostics.Debug.WriteLine($"HTTP Response Status: {response.StatusCode} ({(int)response.StatusCode})");
            _logger?.LogInformation("HTTP Response Status: {StatusCode} ({StatusCodeNumber})",
                response.StatusCode, (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger?.LogWarning("Failed to check for updates: {StatusCode}. Response: {ResponseBody}",
                    response.StatusCode, responseBody);

                // 404 typically means no releases published yet
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger?.LogWarning("No releases found (404). This means no releases have been published to GitHub yet.");
                    return UpdateCheckResult.NoUpdate("No updates available online");
                }
                return UpdateCheckResult.NoUpdate("Unable to connect to update server");
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Received JSON response (length: {json.Length} chars)");
            System.Diagnostics.Debug.WriteLine($"JSON Response: {(json.Length > 1000 ? json.Substring(0, 1000) + "..." : json)}");
            _logger?.LogInformation("Received JSON response (length: {Length} chars)", json.Length);
            _logger?.LogDebug("JSON Response: {Json}", json.Length > 500 ? json.Substring(0, 500) + "..." : json);

            var release = JsonSerializer.Deserialize<GitHubRelease>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (release == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Failed to deserialize GitHub release JSON");
                _logger?.LogError("Failed to deserialize GitHub release JSON");
                return UpdateCheckResult.NoUpdate("Unable to read update information");
            }

            System.Diagnostics.Debug.WriteLine($"Release Info: Tag={release.TagName}, Name={release.Name}, Assets={release.Assets?.Length ?? 0}");
            _logger?.LogInformation("Release Info: Tag={TagName}, Name={Name}, Assets={AssetCount}",
                release.TagName, release.Name, release.Assets?.Length ?? 0);

            // Parse version from tag (expecting format like "v1.0.0" or "1.0.0")
            var latestVersion = release.TagName?.TrimStart('v') ?? "0.0.0";
            var currentVersion = CurrentVersion;

            _logger?.LogInformation("Version Comparison: Latest={LatestVersion}, Current={CurrentVersion}",
                latestVersion, currentVersion);

            if (IsNewerVersion(latestVersion, currentVersion))
            {
                _logger?.LogInformation("✓ UPDATE AVAILABLE: {LatestVersion} (current: {CurrentVersion})",
                    latestVersion, currentVersion);

                // Find the appropriate asset for this platform
                var asset = FindAssetForPlatform(release);

                if (asset != null)
                {
                    _logger?.LogInformation("Found platform asset: {AssetName} ({Size} bytes)",
                        asset.Name, asset.Size);
                }
                else
                {
                    _logger?.LogWarning("No platform-specific asset found in release");
                }

                // For private repos, use the API URL instead of browser_download_url
                string? downloadUrl = asset?.Url ?? asset?.BrowserDownloadUrl;

                System.Diagnostics.Debug.WriteLine($"Asset URL (API): {asset?.Url}");
                System.Diagnostics.Debug.WriteLine($"Asset BrowserDownloadUrl: {asset?.BrowserDownloadUrl}");
                System.Diagnostics.Debug.WriteLine($"Using download URL: {downloadUrl}");

                return new UpdateCheckResult
                {
                    IsUpdateAvailable = true,
                    LatestVersion = latestVersion,
                    CurrentVersion = currentVersion,
                    ReleaseNotes = release.Body ?? "No release notes available",
                    DownloadUrl = downloadUrl,
                    AssetName = asset?.Name,
                    PublishedAt = release.PublishedAt
                };
            }
            else
            {
                _logger?.LogInformation("✗ No update available. Current version {CurrentVersion} is up to date",
                    currentVersion);
                return UpdateCheckResult.NoUpdate("You have the latest version");
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger?.LogError(httpEx, "HTTP ERROR checking for updates: {Message}", httpEx.Message);
            return UpdateCheckResult.NoUpdate($"Network error: {httpEx.Message}");
        }
        catch (JsonException jsonEx)
        {
            _logger?.LogError(jsonEx, "JSON PARSING ERROR: {Message}", jsonEx.Message);
            return UpdateCheckResult.NoUpdate("Error parsing update information");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UNEXPECTED ERROR checking for updates: {ExceptionType} - {Message}",
                ex.GetType().Name, ex.Message);
            _logger?.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            return UpdateCheckResult.NoUpdate($"Error: {ex.Message}");
        }
        finally
        {
            _logger?.LogInformation("========== AUTO-UPDATE CHECK COMPLETE ==========");
        }
    }

    /// <summary>
    /// Download the update to a temporary location
    /// </summary>
    public async Task<DownloadResult> DownloadUpdateAsync(string downloadUrl, IProgress<int>? progress = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Starting download from: {downloadUrl}");
            _logger?.LogInformation("Downloading update from {Url}", downloadUrl);

            var tempPath = Path.Combine(Path.GetTempPath(), "Desktop Food CostUpdate");
            Directory.CreateDirectory(tempPath);

            // Try to get filename from URL first (fallback)
            var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Initial file name from URL: {fileName}");

            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Temp path: {tempPath}");

            string filePath = ""; // Will be set after we determine the final filename

            // Determine if this is an API URL or direct download URL
            bool isApiUrl = downloadUrl.Contains("api.github.com");
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Is API URL: {isApiUrl}");

            // Create request with proper headers for GitHub release assets
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            request.Headers.Add("Accept", "application/octet-stream");

            // Ensure token is in the request (sometimes it needs to be explicit)
            if (!string.IsNullOrWhiteSpace(GitHubToken) && !request.Headers.Contains("Authorization"))
            {
                request.Headers.Add("Authorization", $"token {GitHubToken}");
                System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Added Authorization header explicitly");
            }

            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Request URL: {downloadUrl}");
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Accept header: application/octet-stream");
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Has Auth header: {request.Headers.Contains("Authorization")}");

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Error response body: {errorBody}");
                }

                response.EnsureSuccessStatusCode();

                // Try to get filename from Content-Disposition header
                if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                    System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Got filename from Content-Disposition: {fileName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] No Content-Disposition header, using URL filename: {fileName}");
                    // For API URLs that don't have a proper filename, use a default
                    if (!fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = "Desktop Food CostSetup.exe";
                        System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] URL filename not valid, using default: {fileName}");
                    }
                }

                filePath = Path.Combine(tempPath, fileName);
                System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Final file path: {filePath}");

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[81920]; // 80KB buffer for smoother progress
                int bytesRead;
                int lastReportedPercent = -1;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        var percentComplete = (int)((downloadedBytes * 100) / totalBytes);
                        // Only report progress when it changes by at least 1%
                        if (percentComplete != lastReportedPercent)
                        {
                            progress?.Report(percentComplete);
                            lastReportedPercent = percentComplete;
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Download completed successfully!");
            _logger?.LogInformation("Update downloaded successfully to {FilePath}", filePath);

            return new DownloadResult
            {
                IsSuccess = true,
                FilePath = filePath,
                FileName = fileName
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] ERROR: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DownloadUpdate] Stack: {ex.StackTrace}");
            _logger?.LogError(ex, "Error downloading update");
            return DownloadResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Install the downloaded update (platform-specific)
    /// </summary>
    public Task<bool> InstallUpdateAsync(string updateFilePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Starting install from: {updateFilePath}");
            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] File exists: {File.Exists(updateFilePath)}");

            if (File.Exists(updateFilePath))
            {
                var fileInfo = new FileInfo(updateFilePath);
                System.Diagnostics.Debug.WriteLine($"[InstallUpdate] File size: {fileInfo.Length} bytes");
                System.Diagnostics.Debug.WriteLine($"[InstallUpdate] File extension: {fileInfo.Extension}");
            }

            _logger?.LogInformation("Installing update from {FilePath}", updateFilePath);

            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Platform: Windows");

                // For Windows: Launch the installer/setup exe
                if (updateFilePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[InstallUpdate] File is .exe, launching installer...");

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = updateFilePath,
                        UseShellExecute = true
                    };

                    var process = Process.Start(startInfo);
                    System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Process started: {process != null}");

                    return Task.FromResult(true);
                }
                else if (updateFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    // For portable version, extract and restart
                    var extractPath = Path.GetDirectoryName(updateFilePath);
                    if (extractPath != null)
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(updateFilePath, extractPath, true);
                        return Task.FromResult(true);
                    }
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                // For macOS: Open the DMG or extract ZIP
                if (updateFilePath.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start("open", updateFilePath);
                    return Task.FromResult(true);
                }
                else if (updateFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    var extractPath = Path.GetDirectoryName(updateFilePath);
                    if (extractPath != null)
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(updateFilePath, extractPath, true);
                        return Task.FromResult(true);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[InstallUpdate] ERROR: File is not .exe or .zip, cannot install");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Returning false - no valid installer found");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] EXCEPTION: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[InstallUpdate] Stack: {ex.StackTrace}");
            _logger?.LogError(ex, "Error installing update");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Open the GitHub releases page in browser
    /// </summary>
    public void OpenReleasesPage()
    {
        var url = $"https://github.com/{Owner}/{Repository}/releases";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open releases page");
        }
    }

    /// <summary>
    /// Get the current application version from assembly
    /// </summary>
    private static string GetCurrentVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version;

            System.Diagnostics.Debug.WriteLine($"[GetCurrentVersion] Assembly: {assembly?.FullName}");
            System.Diagnostics.Debug.WriteLine($"[GetCurrentVersion] Version object: {version}");

            if (version != null)
            {
                // Return SemVer format: MAJOR.MINOR.PATCH (e.g., "0.9.0")
                var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
                System.Diagnostics.Debug.WriteLine($"[GetCurrentVersion] Returning version: {versionString}");
                return versionString;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GetCurrentVersion] ERROR: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("[GetCurrentVersion] Falling back to default: 1.0.0");
        return "1.0.0";
    }

    private bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[IsNewerVersion] Comparing: Latest='{latestVersion}' vs Current='{currentVersion}'");
            var latest = Version.Parse(latestVersion);
            var current = Version.Parse(currentVersion);
            var isNewer = latest > current;
            System.Diagnostics.Debug.WriteLine($"[IsNewerVersion] Parsed: Latest={latest} vs Current={current}");
            System.Diagnostics.Debug.WriteLine($"[IsNewerVersion] Result: IsNewer={isNewer}");
            return isNewer;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IsNewerVersion] ERROR parsing versions: {ex.Message}");
            // If version parsing fails, assume no update
            return false;
        }
    }

    private GitHubAsset? FindAssetForPlatform(GitHubRelease release)
    {
        if (release.Assets == null || release.Assets.Length == 0)
            return null;

        // Determine platform
        string platformPattern;
        if (OperatingSystem.IsWindows())
        {
            platformPattern = "win"; // Look for win-x64, win-x86, windows, etc.
        }
        else if (OperatingSystem.IsMacOS())
        {
            platformPattern = "osx"; // Look for osx-x64, macos, etc.
        }
        else
        {
            return null; // Linux not supported yet
        }

        // Find matching asset
        foreach (var asset in release.Assets)
        {
            if (asset.Name != null &&
                asset.Name.Contains(platformPattern, StringComparison.OrdinalIgnoreCase))
            {
                return asset;
            }
        }

        // If no platform-specific asset found, return first asset
        return release.Assets.Length > 0 ? release.Assets[0] : null;
    }
}

/// <summary>
/// Interface for auto-update service
/// </summary>
public interface IAutoUpdateService
{
    string Owner { get; set; }
    string Repository { get; set; }
    Task<UpdateCheckResult> CheckForUpdateAsync();
    Task<DownloadResult> DownloadUpdateAsync(string downloadUrl, IProgress<int>? progress = null);
    Task<bool> InstallUpdateAsync(string updateFilePath);
    void OpenReleasesPage();
}

/// <summary>
/// Result of checking for updates
/// </summary>
public class UpdateCheckResult
{
    public bool IsUpdateAvailable { get; set; }
    public string? LatestVersion { get; set; }
    public string? CurrentVersion { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? DownloadUrl { get; set; }
    public string? AssetName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Message { get; set; }

    public static UpdateCheckResult NoUpdate(string message) => new()
    {
        IsUpdateAvailable = false,
        Message = message
    };
}

/// <summary>
/// Result of downloading an update
/// </summary>
public class DownloadResult
{
    public bool IsSuccess { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }

    public static DownloadResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}

/// <summary>
/// GitHub release model
/// </summary>
public class GitHubRelease
{
    [System.Text.Json.Serialization.JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("body")]
    public string? Body { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("assets")]
    public GitHubAsset[]? Assets { get; set; }
}

/// <summary>
/// GitHub release asset model
/// </summary>
public class GitHubAsset
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public long Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("url")]
    public string? Url { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadUrl { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("size")]
    public long Size { get; set; }
}

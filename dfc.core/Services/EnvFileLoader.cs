using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dfc.Core.Services;

/// <summary>
/// Loads environment variables from .env file
/// </summary>
public static class EnvFileLoader
{
    /// <summary>
    /// Loads environment variables from .env file
    /// Searches in current directory and up to 3 parent directories
    /// </summary>
    public static Dictionary<string, string> Load()
    {
        var envVars = new Dictionary<string, string>();

        // Try to find .env file in current directory or parent directories
        var searchPaths = new List<string>
        {
            Directory.GetCurrentDirectory(),
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
            Path.Combine(Directory.GetCurrentDirectory(), ".."),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".."),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."),
        };

        string? envFilePath = null;
        foreach (var searchPath in searchPaths.Where(p => !string.IsNullOrEmpty(p)))
        {
            try
            {
                var fullPath = Path.GetFullPath(searchPath);
                var potentialEnvFile = Path.Combine(fullPath, ".env");

                if (File.Exists(potentialEnvFile))
                {
                    envFilePath = potentialEnvFile;
                    System.Diagnostics.Debug.WriteLine($"[EnvLoader] Found .env file at: {envFilePath}");
                    break;
                }
            }
            catch
            {
                // Ignore path resolution errors
                continue;
            }
        }

        if (envFilePath == null)
        {
            System.Diagnostics.Debug.WriteLine("[EnvLoader] .env file not found in search paths");
            return envVars;
        }

        // Parse .env file
        try
        {
            var lines = File.ReadAllLines(envFilePath);
            foreach (var line in lines)
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Parse KEY=VALUE
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    envVars[key] = value;
                    System.Diagnostics.Debug.WriteLine($"[EnvLoader] Loaded {key} from .env");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EnvLoader] Error reading .env file: {ex.Message}");
        }

        return envVars;
    }
}

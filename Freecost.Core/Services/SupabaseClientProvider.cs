using Supabase;
using Supabase.Gotrue.Interfaces;
using System;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class SupabaseClientProvider
{
    private static Client? _instance;
    private static readonly object _lock = new object();
    private static bool _isInitialized = false;

    public static async Task<Client> GetClientAsync()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
                    var key = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");

                    // Fallback: Try loading from .env file if env vars not set
                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
                    {
                        System.Diagnostics.Debug.WriteLine("[Supabase] Env vars not found, trying .env file...");
                        var envVars = EnvFileLoader.Load();

                        if (envVars.ContainsKey("SUPABASE_URL"))
                            url = envVars["SUPABASE_URL"];
                        if (envVars.ContainsKey("SUPABASE_ANON_KEY"))
                            key = envVars["SUPABASE_ANON_KEY"];
                    }

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
                    {
                        throw new InvalidOperationException(
                            "Supabase credentials not configured! Please set environment variables: SUPABASE_URL and SUPABASE_ANON_KEY, or create a .env file in the project root.");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Supabase] Connecting to: {url}");

                    var options = new SupabaseOptions
                    {
                        AutoRefreshToken = true,
                        AutoConnectRealtime = false,
                        SessionHandler = new SupabaseSessionHandler()
                    };

                    _instance = new Client(url, key, options);
                }
            }
        }

        if (!_isInitialized)
        {
            await _instance.InitializeAsync();
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine("Supabase client initialized successfully");
        }

        return _instance;
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
            _isInitialized = false;
        }
    }

    public static bool IsConnected()
    {
        return _instance != null && _instance.Auth.CurrentUser != null;
    }
}

public class SupabaseSessionHandler : IGotrueSessionPersistence<Supabase.Gotrue.Session>
{
    private readonly string _sessionFilePath;

    public SupabaseSessionHandler()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var freecostDir = System.IO.Path.Combine(appDataPath, "Freecost");
        System.IO.Directory.CreateDirectory(freecostDir);
        _sessionFilePath = System.IO.Path.Combine(freecostDir, "supabase_session.json");
    }

    public void SaveSession(Supabase.Gotrue.Session session)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(session);
            System.IO.File.WriteAllText(_sessionFilePath, json);
        }
        catch
        {
            // Ignore save failures
        }
    }

    public void DestroySession()
    {
        try
        {
            if (System.IO.File.Exists(_sessionFilePath))
            {
                System.IO.File.Delete(_sessionFilePath);
            }
        }
        catch
        {
            // Ignore delete failures
        }
    }

    public Supabase.Gotrue.Session? LoadSession()
    {
        try
        {
            if (System.IO.File.Exists(_sessionFilePath))
            {
                var json = System.IO.File.ReadAllText(_sessionFilePath);
                return System.Text.Json.JsonSerializer.Deserialize<Supabase.Gotrue.Session>(json);
            }
        }
        catch
        {
            // Ignore load failures
        }

        return null;
    }
}

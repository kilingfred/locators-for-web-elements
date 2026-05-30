using System.Text.Json;

public static class Configuration
{
    private static readonly JsonDocument Settings;

    static Configuration()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (File.Exists(path))
        {
            Settings = JsonDocument.Parse(File.ReadAllText(path));
        }
    }

    public static string Browser =>
        Environment.GetEnvironmentVariable("BROWSER")
        ?? GetString("browser")
        ?? "chrome";

    public static bool Headless =>
        bool.TryParse(GetString("Headless"), out var result) && result;

    private static string? GetString(string key)
    {
        if (Settings == null)
            return null;

        return Settings.RootElement.TryGetProperty(key, out var value)
            ? value.ToString()
            : null;
    }
}
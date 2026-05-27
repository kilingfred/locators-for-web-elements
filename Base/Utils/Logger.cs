using System;
using System.IO;
using System.Text.Json;

namespace Base.Utils
{
    public static class Logger
    {
        private static readonly object _sync = new object();
        private static string _logFilePath;
        private static LogLevel _level = LogLevel.Info;

        static Logger()
        {
            TryLoadConfiguration();
            // ensure default path if not configured
            if (string.IsNullOrEmpty(_logFilePath))
            {
                var baseDir = AppContext.BaseDirectory;
                _logFilePath = Path.GetFullPath(Path.Combine(baseDir, "logs", "tests.log"));
                var dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        private static void TryLoadConfiguration()
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var settingsPath = Path.Combine(baseDir, "appsettings.json");
                if (!File.Exists(settingsPath))
                    return;

                using var stream = File.OpenRead(settingsPath);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("Logging", out var logging))
                {
                    if (logging.TryGetProperty("LogFile", out var lf) && lf.ValueKind == JsonValueKind.String)
                    {
                        _logFilePath = Path.GetFullPath(Path.Combine(baseDir, lf.GetString()));
                        var dir = Path.GetDirectoryName(_logFilePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }

                    if (logging.TryGetProperty("Level", out var lvl) && lvl.ValueKind == JsonValueKind.String)
                    {
                        Enum.TryParse<LogLevel>(lvl.GetString(), true, out _level);
                    }
                }
            }
            catch
            {
                // swallow config errors - fallback to Console
            }
        }

        private static void Write(LogLevel level, string message)
        {
            if (level < _level) return;
            var text = $"{DateTime.UtcNow:O} [{level}] {message}";
            lock (_sync)
            {
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    try { File.AppendAllText(_logFilePath, text + Environment.NewLine); } catch { }
                }
                Console.WriteLine(text);
            }
        }

        public static void Debug(string message) => Write(LogLevel.Debug, message);
        public static void Info(string message) => Write(LogLevel.Info, message);
        public static void Warn(string message) => Write(LogLevel.Warn, message);
        public static void Error(string message) => Write(LogLevel.Error, message);
        public static void Error(string message, Exception ex) => Write(LogLevel.Error, message + " | Exception: " + ex);

        // Clear or create log file. Intended to be called before test run.
        public static void Clear()
        {
            lock (_sync)
            {
                try
                {
                    if (string.IsNullOrEmpty(_logFilePath))
                    {
                        var baseDir = AppContext.BaseDirectory;
                        _logFilePath = Path.GetFullPath(Path.Combine(baseDir, "logs", "tests.log"));
                    }
                    var dir = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(_logFilePath, string.Empty);
                }
                catch
                {
                    // ignore
                }
            }
        }

        private enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3
        }
    }
}

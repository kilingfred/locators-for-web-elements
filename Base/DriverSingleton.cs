using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenQA.Selenium;
using Base.Utils;

namespace Base
{
    // Simple singleton managing a single IWebDriver instance per test run.
    public static class DriverSingleton
    {
        private static IWebDriver _driver;
        private static IDriverFactory _factory;
        private static readonly object _sync = new object();

        // Create or return existing driver. Factory selection is handled here based on BROWSER env var
        // or the "browser" key in appsettings.json. Environment variable takes precedence.
        public static IWebDriver GetDriver()
        {
            lock (_sync)
            {
                if (_driver != null) return _driver;

                // decide which factory to use
                var browser = Environment.GetEnvironmentVariable("BROWSER");
                if (string.IsNullOrWhiteSpace(browser))
                {
                    var configured = GetConfiguredBrowser();
                    browser = configured;
                }

                browser = (browser ?? "chrome").ToLowerInvariant();

                _factory = browser switch
                {
                    "edge" => new EdgeDriverFactory(),
                    _ => new ChromeDriverFactory(),
                };

                _driver = _factory.GetDriver();
                Logger.Info($"DriverSingleton created driver instance using {browser}");
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                return _driver;
            }
        }

        // Backwards-compatible overload when caller provides factory explicitly.
        public static IWebDriver GetDriver(IDriverFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            lock (_sync)
            {
                if (_driver != null) return _driver;
                _factory = factory;
                _driver = _factory.GetDriver();
                Logger.Info("DriverSingleton created driver instance via provided factory");
                return _driver;
            }
        }

        public static void CloseAndClear()
        {
            lock (_sync)
            {
                if (_driver == null) return;
                try
                {
                    _factory?.CloseDriver(_driver);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error closing driver in singleton", ex);
                }
                finally
                {
                    try { _driver.Dispose(); } catch { }
                    _driver = null;
                    _factory = null;
                }
            }
        }

        public static string DownloadDirectory
        {
            get
            {
                // Ensure factory (and driver) are initialized before returning download directory.
                // This avoids callers getting null if they query the property before the
                // singleton has created the driver instance.
                if (_factory == null)
                {
                    try { GetDriver(); } catch { /* swallow - caller will get null if creation fails */ }
                }
                return _factory?.DownloadDirectory;
            }
        }

        // Try to read "browser" from appsettings.json located in the current working directory
        // or application base directory. Returns null on failure.
        private static string GetConfiguredBrowser()
        {
            try
            {
                // Search upwards from current directory to repository/solution root to find appsettings.json
                var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "appsettings.json");
                    if (File.Exists(candidate))
                    {
                        using var stream = File.OpenRead(candidate);
                        using var doc = JsonDocument.Parse(stream);
                        if (doc.RootElement.TryGetProperty("browser", out var prop))
                        {
                            var val = prop.GetString();
                            return string.IsNullOrWhiteSpace(val) ? null : val;
                        }
                        return null;
                    }
                    dir = dir.Parent;
                }

                // Fallback to AppContext.BaseDirectory (e.g., when tests are executed from build output)
                var fallback = Path.Combine(AppContext.BaseDirectory ?? string.Empty, "appsettings.json");
                if (File.Exists(fallback))
                {
                    using var stream = File.OpenRead(fallback);
                    using var doc = JsonDocument.Parse(stream);
                    if (doc.RootElement.TryGetProperty("browser", out var prop))
                    {
                        var val = prop.GetString();
                        return string.IsNullOrWhiteSpace(val) ? null : val;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Unable to read browser from appsettings.json: " + ex.Message);
            }

            return null;
        }
    }
}

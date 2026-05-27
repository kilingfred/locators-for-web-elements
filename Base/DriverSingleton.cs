using System;
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

        // Create or return existing driver. Factory selection is handled here based on BROWSER env var.
        public static IWebDriver GetDriver()
        {
            lock (_sync)
            {
                if (_driver != null) return _driver;

                // decide which factory to use
                var browser = Environment.GetEnvironmentVariable("BROWSER")?.ToLowerInvariant() ?? "chrome";
                _factory = browser switch
                {
                    "firefox" => new FirefoxDriverFactory(),
                    "edge" => new EdgeDriverFactory(),
                    _ => new ChromeDriverFactory(),
                };

                _driver = _factory.GetDriver();
                Logger.Info($"DriverSingleton created driver instance using {browser}");
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
    }
}

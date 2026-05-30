using OpenQA.Selenium;
using Base.Utils;
using Base.DriverFactories;

namespace Base
{
    public static class DriverSingleton
    {
        private static IWebDriver _driver;
        private static IDriverFactory _factory;
        private static readonly Lock _sync = new();

        public static IWebDriver GetDriver()
        {
            lock (_sync)
            {
                if (_driver != null)
                    return _driver;

                var browser =
                    Environment.GetEnvironmentVariable("BROWSER")
                    ?? Configuration.Browser
                    ?? "chrome";

                browser = browser.ToLowerInvariant();

                Logger.Info($"Selected browser: {browser}");

                _factory = browser switch
                {
                    "edge" => new EdgeDriverFactory(),
                    _ => new ChromeDriverFactory()
                };

                _driver = _factory.GetDriver();

                Logger.Info($"DriverSingleton created driver instance using {browser}");

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                return _driver;
            }
        }

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
                if (_factory == null)
                {
                    try { GetDriver(); } catch { /* swallow - caller will get null if creation fails */ }
                }
                return _factory?.DownloadDirectory;
            }
        }
    }
}

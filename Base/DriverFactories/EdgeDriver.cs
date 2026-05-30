using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Base.Utils;

namespace Base.DriverFactories
{
    public class EdgeDriverFactory : IDriverFactory
    {
        private IWebDriver driver;

        public string DownloadDirectory { get; private set; }

        public IWebDriver GetDriver()
        {
            DownloadDirectory = Path.Combine(Path.GetTempPath(), "epam_downloads", Guid.NewGuid().ToString());
            Directory.CreateDirectory(DownloadDirectory);

            var options = new EdgeOptions();
            options.AddUserProfilePreference("download.default_directory", DownloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            if (!Configuration.Headless)
            {
                driver.Manage().Window.Maximize();
            }
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");

            driver = new OpenQA.Selenium.Edge.EdgeDriver(options);
            if (!Configuration.Headless)
            {
                driver.Manage().Window.Maximize();
            }
            Logger.Info("EdgeDriver started");
            return driver;
        }

        public void CloseDriver(IWebDriver driver)
        {
            try
            {
                Logger.Info("Closing WebDriver");
                driver.Quit();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while quitting driver", ex);
            }
            finally
            {
                try
                {
                    driver.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while disposing driver", ex);
                }
            }
        }
    }
}

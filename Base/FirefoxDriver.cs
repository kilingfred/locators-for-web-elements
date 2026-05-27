using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Base.Utils;

namespace Base
{
    public class FirefoxDriverFactory : IDriverFactory
    {
        private IWebDriver driver;

        public string DownloadDirectory { get; private set; }

        public IWebDriver GetDriver()
        {
            DownloadDirectory = Path.Combine(Path.GetTempPath(), "epam_downloads", Guid.NewGuid().ToString());
            Directory.CreateDirectory(DownloadDirectory);

            var options = new FirefoxOptions();
            options.SetPreference("browser.download.dir", DownloadDirectory);
            options.SetPreference("browser.download.folderList", 2);
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/pdf");

            driver = new OpenQA.Selenium.Firefox.FirefoxDriver(options);
            driver.Manage().Window.Maximize();
            Logger.Info("FirefoxDriver started");
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

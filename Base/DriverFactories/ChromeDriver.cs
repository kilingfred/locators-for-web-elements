using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Runtime.CompilerServices;

namespace Base.DriverFactories
{
    public class ChromeDriverFactory : IDriverFactory
    {
        private IWebDriver driver;

        public string DownloadDirectory { get; private set; }

        public IWebDriver GetDriver()
        {
            DownloadDirectory = Path.Combine(Path.GetTempPath(), "epam_downloads", Guid.NewGuid().ToString());
            Directory.CreateDirectory(DownloadDirectory);

            var options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", DownloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            driver = new OpenQA.Selenium.Chrome.ChromeDriver(options);
            driver.Manage().Window.Maximize();
            Base.Utils.Logger.Info("ChromeDriver started");
            return driver;
        }

        public void CloseDriver(IWebDriver driver)
        {
            try
            {
                Base.Utils.Logger.Info("Closing WebDriver");
                driver.Quit();
            }
            catch (Exception ex)
            {
                Base.Utils.Logger.Error("Error while quitting driver", ex);
            }
            finally
            {
                try
                {
                    driver.Dispose();
                }
                catch (Exception ex)
                {
                    Base.Utils.Logger.Error("Error while disposing driver", ex);
                }
            }
        }
    }
}

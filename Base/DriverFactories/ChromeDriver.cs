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
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            // Ensure a desktop-sized viewport in headless mode so responsive layout matches local runs
            options.AddArgument("--window-size=1920,1080");
            // Allow newer Chrome/Chromedriver combinations to work without remote origin errors on some CI images
            options.AddArgument("--remote-allow-origins=*");
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
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

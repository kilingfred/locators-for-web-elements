using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Runtime.CompilerServices;

namespace Base
{
    public static class Driver
    {
        private static IWebDriver driver;
       
        public static IWebDriver GetDriver() 
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            driver.Manage().Window.Maximize();
            return driver;
        }
    }
}

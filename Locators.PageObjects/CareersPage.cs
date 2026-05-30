using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Base.Utils;

namespace Locators.PageObjects
{
    public class CareersPage
    {
        private const string URL = "https://www.epam.com/careers";
        private IWebDriver webDriver;
        private WebDriverWait webDriverWait;
        
        private By StartYourSearchHereButtonLocator = By.CssSelector("#main > div.content-container.parsys > div:nth-child(1) > section > div.section__wrapper.section--padding-no > div.layout-box > div > div > div > div > div.pinned-button > div > a");

        public CareersPage(IWebDriver driver)
        {
            this.webDriver = driver;
            this.webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
        }

        private IWebElement StartYourSearchHereElement => this.webDriverWait.Until(driver => driver.FindElement(StartYourSearchHereButtonLocator));

        public CareersPage Open()
        {
            Logger.Info($"Navigating to {URL}");
            this.webDriver.Navigate().GoToUrl(URL);
            return this;
        }

        public CareersJobsPage GoToCareersJobsPage()
        {
            try
            {
                StartYourSearchHereElement.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].click();", StartYourSearchHereElement);
            }
            return new CareersJobsPage(this.webDriver);
        }
    }
}

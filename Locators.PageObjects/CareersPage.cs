using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

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
            this.webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(3));
        }

        private IWebElement StartYourSearchHereElement => this.webDriverWait.Until(driver => driver.FindElement(StartYourSearchHereButtonLocator));

        public CareersPage Open()
        {
            this.webDriver.Navigate().GoToUrl(URL);
            return this;
        }

        public CareersUkrainePage GoToCareersUkrainePage()
        {
            StartYourSearchHereElement.Click();
            return new CareersUkrainePage(this.webDriver);
        }
    }
}

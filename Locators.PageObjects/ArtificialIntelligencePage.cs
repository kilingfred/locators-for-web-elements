using Base.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Locators.PageObjects
{
    public class ArtificialIntelligencePage
    {
        private const string URL = "https://www.epam.com/services/artificial-intelligence/";
        private IWebDriver webDriver;
        private WebDriverWait webDriverWait;
        private By relatedExpertiseLocator = By.XPath("//*[contains(normalize-space(.), 'Our Related Expertise')]");

        private string Title { get; set; }
        private IWebElement RelatedExpertiseElement => this.webDriverWait.Until(driver => driver.FindElement(relatedExpertiseLocator));

        public ArtificialIntelligencePage(IWebDriver webDriver, string title)
        {
            this.webDriver = webDriver;
            this.Title = title;
            this.webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
        }

        public ArtificialIntelligencePage Open()
        {
            Logger.Info($"Opening {URL + Title}");
            this.webDriver.Navigate().GoToUrl(URL + Title.Replace(' ', '-').ToLower());
            return this;
        }

        public string FindTitle()
        {
            Logger.Info("Trying to find page title");
            // ClassName cannot contain spaces; use CSS selector for compound classes
            return webDriverWait.Until(driver => driver.FindElement(By.CssSelector(".museo-sans-500.gradient-text"))).Text;
        }

        public bool RelatedExpertiseIsPresent()
        {
            Actions actions = new Actions(this.webDriver);
            actions.ScrollToElement(RelatedExpertiseElement).Perform();
            return RelatedExpertiseElement.Displayed;
        }
    }
}

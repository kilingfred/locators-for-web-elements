using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.BrowsingContext;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Base;
using System.Diagnostics;
using Base.Utils;
using System.Linq;

namespace Locators.PageObjects
{
    public class MainPage
    {
        private const string URL = "https://www.epam.com/";
        private IWebDriver webDriver;
        private WebDriverWait webDriverWait;
       
        private By careerLocator = By.XPath("//*[@id=\"wrapper\"]/div[2]/div[1]/header/div/div/nav/ul/li[5]/span[1]/a");
        private By searchLocator = By.CssSelector("#wrapper > div.header-container.iparsys.parsys > div.header.section > header > div > div > ul > li:nth-child(3) > div > button");
        private By inputLocator = By.TagName("input");
        private By searchButtonLocator = By.CssSelector("#wrapper > div.header-container.iparsys.parsys > div.header.section > header > div > div > ul > li:nth-child(3) > div > div > form > div.search-results__action-section > button");

        public MainPage(IWebDriver webDriver)
        {
            this.webDriver = webDriver;
            webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(3));
        }

        IWebElement SearchElement => this.webDriverWait.Until(driver => driver.FindElement(searchLocator));
        IWebElement InputElement => this.webDriverWait.Until(driver => driver.FindElement(inputLocator));
        IWebElement SearchButtonElement => this.webDriverWait.Until(driver => driver.FindElement(searchButtonLocator));
        string Text { get; set; }

        public MainPage Open()
        {
            webDriver.Navigate().GoToUrl(URL);
            // Wait for document ready state to reduce chance of interacting with not-ready elements
            try
            {
                var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
            }
            catch (Exception ex) { Logger.Warn("Document ready state wait failed: " + ex.Message); }
            return this;
        }

        public CareersPage GoToCareersPage()
        {
            // Wait until element is visible and enabled to avoid ElementNotInteractableException (overlay or layout changes)
            var clickable = webDriverWait.Until(driver =>
            {
                var e = driver.FindElement(careerLocator);
                return (e.Displayed && e.Enabled) ? e : null;
            });
            if (clickable == null) Logger.Warn("Careers link not found or not clickable on MainPage");

            // Scroll into view in case header layout or sticky elements cover it
            ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].scrollIntoView(true);", clickable);

            clickable.Click();
            return new CareersPage(this.webDriver);
        }

        public MainPage ClickSearchIcon()
        {
            this.SearchElement.Click();
            return this;
        }

        public MainPage InputToSearchField(string text)
        {
            // Prefer a visible, enabled input. There may be multiple inputs on the page and some are hidden or offscreen.
            var inputs = webDriver.FindElements(By.TagName("input"));
            var target = inputs.FirstOrDefault(e => e.Displayed && e.Enabled && e.Size.Width > 0 && e.Size.Height > 0);

            if (target == null)
            {
                // Wait briefly for a visible input to appear
                webDriverWait.Until(d => d.FindElements(By.TagName("input")).Any(i => i.Displayed && i.Enabled && i.Size.Width > 0 && i.Size.Height > 0));
                inputs = webDriver.FindElements(By.TagName("input"));
                target = inputs.FirstOrDefault(e => e.Displayed && e.Enabled && e.Size.Width > 0 && e.Size.Height > 0);
            }

            if (target == null)
            {
                // Last resort: use the original InputElement (may throw)
                target = this.InputElement;
            }

            try
            {
                target.Clear();
                target.SendKeys(text);
            }
            catch (ElementNotInteractableException)
            {
                // Fallback: set value via JavaScript and dispatch input events to simulate typing
                try
                {
                    ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input'));", target, text);
                }
                catch
                {
                    // rethrow original exception to preserve test failure information
                    throw;
                }
            }

            this.Text = text;
            return this;
        }

        public SearchPage ClickSearch()
        {
            this.SearchButtonElement.Click();
            return new SearchPage(webDriver, Text);
        }
    }
}

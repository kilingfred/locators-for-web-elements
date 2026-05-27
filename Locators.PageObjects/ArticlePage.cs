using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Base.Utils;
using System;

namespace Locators.PageObjects
{
    public class ArticlePage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        // Use //h1 to find the element anywhere in the document
        private readonly By pageHeader = By.TagName("h1");

        public ArticlePage(IWebDriver driver, string path)
        {
            this.driver = driver;
            this.Path = path;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            Logger.Debug($"ArticlePage initialized for path: {path}");
        }

        private string Path { get; init; }

        // Use a lazy property that waits for visibility
        private IWebElement PageHeaderElement => wait.Until(d => {
            var el = d.FindElement(pageHeader);
            Logger.Debug("Located article page header element");
            return el;
        });

        public ArticlePage Open()
        {
            Logger.Info($"Navigating to article URL: {Path}");
            driver.Navigate().GoToUrl(Path);
            return this;
        }

        public string GetArticleTitle()
        {
            var title = PageHeaderElement.Text.Trim();
            Logger.Debug($"Article title determined: '{title}'");
            return title;
        }
    }
}
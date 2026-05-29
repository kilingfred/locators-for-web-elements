using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Locators.PageObjects
{
    public class SearchPage
    {
        private const string URL = "https://www.epam.com/search";
        private IWebDriver webDriver;
        private WebDriverWait webDriverWait;
        private string queryText;

        private By searchResultsLocator = By.ClassName("search-results__items");

        public SearchPage(IWebDriver webDriver, string queryText)
        {
            this.queryText = queryText;
            this.webDriver = webDriver;
            webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(3));
        }

        private IWebElement SearchResultsElement => webDriverWait.Until(driver => driver.FindElement(searchResultsLocator));

        public SearchPage Open()
        {
            webDriver.Navigate().GoToUrl(URL + "?q=" + queryText);
            return this;
        }

        public bool checkIfEachArticleHasQueryText()
        {
            try
            {
                // Inspect each logical search-result item (to avoid unrelated links like pagination or actions)
                var items = SearchResultsElement.FindElements(By.CssSelector(".search-results__item"));
                var texts = new System.Collections.Generic.List<string>();
                foreach (var item in items)
                    texts.Add(item.Text ?? string.Empty);

                var unmatched = texts.FindAll(t => !t.Contains(queryText, StringComparison.OrdinalIgnoreCase));
                if (unmatched.Count > 0)
                {
                    Base.Utils.Logger.Warn($"Search check: query='{queryText}', totalAnchors={items.Count}, unmatchedSamples={string.Join("; ", unmatched.Take(5))}");
                }

                return unmatched.Count == 0;
            }
            catch (Exception ex)
            {
                Base.Utils.Logger.Error("Error while checking search results", ex);
                throw;
            }
        }
    }
}

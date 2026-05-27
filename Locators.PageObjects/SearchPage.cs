using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

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
                return SearchResultsElement.FindElements(By.TagName("a")).All(e => e.Text.Contains(queryText));
            }
            catch (Exception ex)
            {
                Base.Utils.Logger.Error("Error while checking search results", ex);
                throw;
            }
        }
    }
}

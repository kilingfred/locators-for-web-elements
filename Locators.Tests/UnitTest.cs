using NUnit.Framework;
using OpenQA.Selenium;
using Base;
using Locators.PageObjects;

namespace Locators.Tests
{
    public class Tests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = Driver.GetDriver();
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                try { driver.Quit(); } catch { }
                try { driver.Dispose(); } catch { }
                driver = null;
            }
        }

        // Test 1: Careers Ukraine search flow
        [TestCase("Java", "Ukraine")]
        [TestCase("Python", "Ukraine")]
        public void CareersUkraine_SearchAndExpand_LastItemContainsSearchText(string language, string country)
        {
            var main = new MainPage(driver).Open();
            var careersPage = main.GoToCareersPage();
            var careersUa = careersPage.GoToCareersUkrainePage();

            // Ensure page loaded (Open navigates directly if needed)
            careersUa.Open()
                .SelectCountryFromTheDropdownList(country)
                .InputSearchText(language)
                .SelectRemote()
                .ExpandLastJobFromList();

            Assert.That(careersUa.ContainsSearchedText(), NUnit.Framework.Is.True, $"Expected last job to contain search text '{language}'");
        }

        // Test 2: Site-wide search validating that all results contain the query text
        [TestCase("BLOCKCHAIN")]
        [TestCase("Cloud")]
        [TestCase("Automation")]
        public void MainSearch_AllResultsContainQuery(string query)
        {
            var main = new MainPage(driver).Open();
            var searchPage = main
                .ClickSearchIcon()
                .InputToSearchField(query)
                .ClickSearch();

            // Ensure results page is loaded with the query
            searchPage.Open();

            Assert.That(searchPage.checkIfEachArticleHasQueryText(), NUnit.Framework.Is.True, $"Not every result contains '{query}'");
        }
    }
}

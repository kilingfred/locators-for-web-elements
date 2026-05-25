using Base;
using Locators.PageObjects;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Threading;

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
                try
                {
                    // 1. Close all windows and end session
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during driver.Quit(): {ex.Message}");
                }
                finally
                {
                    // 2. Ensure disposal regardless of Quit success
                    driver.Dispose();
                    driver = null;
                }
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
                .InputSearchText(language)
                .SelectCountryFromTheDropdownList(country)
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

        [Test]
        public void Insights_CarouselArticleTitle_MatchesDetailedPage()
        {
            var insightsPage = new InsightsPage(driver).Open();

            insightsPage.AdvanceCarousel(2);
            string carouselTitle = insightsPage.GetActiveSlideTitle();
            Assert.That(carouselTitle, Is.Not.Null.And.Not.Empty, "Unable to get carousel title");

            // The click and wait logic is now hidden inside the Page Objects
            var articlePage = insightsPage.ClickReadMoreForActiveSlide();
            string articleTitle = articlePage.GetArticleTitle();

            Assert.That(articleTitle, Is.Not.Null.And.Not.Empty, "Unable to determine article title on detailed page");
            Assert.That(string.Equals(carouselTitle, articleTitle, StringComparison.OrdinalIgnoreCase),
                $"Carousel title '{carouselTitle}' does not match article title '{articleTitle}'");
        }

        [Test]
        public void Footer_Download_CodeOfConduct_Pdf()
        {
            string expectedFileName = "Code-Of-Conduct_01_26.pdf";

            // Assume Driver exposes the download path used during initialization
            string expectedFilePath = Path.Combine(Driver.DownloadDirectory, expectedFileName);

            var footer = new FooterPage(driver).OpenHomePage();
            footer.ScrollToFooter();
            footer.ClickCodeOfConductLink();

            // File system checks belong in a utility class, not a Page Object
            bool isDownloaded = Base.Utils.FileHelper.WaitForFileCreatedAndStable(expectedFilePath, TimeSpan.FromSeconds(60));
            Assert.That(isDownloaded, Is.True, $"Expected file '{expectedFileName}' was not downloaded to '{expectedFilePath}'");
        }


    }
}

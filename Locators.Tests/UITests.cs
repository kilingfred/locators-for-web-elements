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
using Base.Utils;

namespace Locators.Tests
{
    [Category("UI")]
    public class UITests: BaseTest
    {
        // Test 1: Careers Ukraine search flow
        [TestCase("Java", "Bulgaria")]
        [TestCase("Python", "Ukraine")]
        public void Careers_SearchAndExpand_LastItemContainsSearchText(string language, string country)
        {
            var main = new MainPage(driver).Open();
            var careersPage = main.GoToCareersPage();
            var careersUa = careersPage.GoToCareersJobsPage();

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

            // Get download path from singleton factory used to create the driver
            string downloadDirectory = DriverSingleton.DownloadDirectory;
            string expectedFilePath = Path.Combine(downloadDirectory, expectedFileName);

            var footer = new FooterPage(driver).OpenHomePage();
            footer.ScrollToFooter();
            footer.ClickCodeOfConductLink();

            // File system checks belong in a utility class, not a Page Object
            bool isDownloaded = Base.Utils.FileHelper.WaitForFileCreatedAndStable(expectedFilePath, TimeSpan.FromSeconds(60));
            Assert.That(isDownloaded, Is.True, $"Expected file '{expectedFileName}' was not downloaded to '{expectedFilePath}'");
        }
    }
}

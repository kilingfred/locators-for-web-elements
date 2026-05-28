using Base;
using Locators.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;

namespace Locators.Tests.Steps
{
    [Binding]
    public class MainMenuSteps : BaseTest
    {
        private IWebDriver driver;
        private MainPage mainPage;

        public MainMenuSteps()
        {
            driver = DriverSingleton.GetDriver();
            mainPage = new MainPage(driver);
        }

        [Given("I am on Main Page")]
        public void OpenMainPage()
        {
            mainPage.Open();
        }

        [When("I hover over Services")]
        public void HoverOverServices()
        {
            mainPage.HoverOnServices();
        }

        [When("I click on {string}")]
        public void ClickOnService(string service)
        {
            mainPage.ClickOnService(service);
        }
    }
}

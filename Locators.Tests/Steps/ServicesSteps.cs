using Base;
using Locators.PageObjects;
using OpenQA.Selenium;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;

namespace Locators.Tests.Steps
{
    [Binding]
    public class ServicesSteps : BaseTest
    {
        private IWebDriver driver;
        private ArtificialIntelligencePage servicePage;

        public ServicesSteps()
        {
            driver = DriverSingleton.GetDriver();
        }

        [Then("I see the same {string}")]
        public void ValidateCorrectServiceClicked(string service)
        {
            servicePage = new ArtificialIntelligencePage(driver, service);
            Assert.True(servicePage.FindTitle() == service);
        }

        [Then("there is 'Our Related Expertise' section")]
        public void ValidateRelatedExpertiseSectionExists()
        {
            if (servicePage == null)
            {
                // If previous step didn't set it for some reason, bind it now with a generic title
                servicePage = new ArtificialIntelligencePage(driver, "");
            }
            Assert.True(servicePage.RelatedExpertiseIsPresent());
        }
    }
}

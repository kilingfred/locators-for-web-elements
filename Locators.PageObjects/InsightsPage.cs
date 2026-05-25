using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Keep this ONLY if you need it, otherwise remove
using System;
using System.Linq;
using System.Threading;

namespace Locators.PageObjects
{
    public class InsightsPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private const string URL = "https://www.epam.com/insights";

        private By nextButtonLocator = By.CssSelector("#main > div.content-container.parsys > div:nth-child(1) > div > div.slider__navigation > button.slider__right-arrow.slider-navigation-arrow");
        private By activeSlideTitleLocator = By.ClassName("scaling-of-text-wrapper");
        private By readMoreLocator = By.XPath("//*[@id=\"main\"]/div[1]/div[1]/div/div[1]/div[1]/div/div[4]/div/div/div/div[2]/a");
        
        public InsightsPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        IWebElement ReadMoreElement => wait.Until(d => d.FindElement(readMoreLocator));

        public InsightsPage Open()
        {
            driver.Navigate().GoToUrl(URL);
            return this;
        }

        public void AdvanceCarousel(int times)
        {
            for (int i = 0; i < times; i++)
            { 
                IWebElement nextBtn;
                    nextBtn = wait.Until(d => {
                        var elements = d.FindElements(nextButtonLocator);
                        return elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                    });


                    nextBtn.Click();


                // Дождаться появления элемента "Read more" (без присвоения read-only свойству)
                wait.Until(d => d.FindElement(readMoreLocator));
                Thread.Sleep(500);
            }
        }

        public string GetActiveSlideTitle()
        {
            // Wait specifically for the active slide text to be non-empty
            var titleElement = wait.Until(d => {
                var el = d.FindElements(activeSlideTitleLocator).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                return el;
            });
            return titleElement.Text.Trim();
        }

        public ArticlePage ClickReadMoreForActiveSlide()
        {
            IWebElement readMoreElement = null;

            // Primary attempt: existing locator
            try
            {
                readMoreElement = wait.Until(d => {
                    var candidates = d.FindElements(readMoreLocator);
                    return candidates.FirstOrDefault(e => e.Displayed && e.Enabled);
                });
            }
            catch (WebDriverTimeoutException)
            {
                // Fallback: try to find a link inside the active slide title container
                try
                {
                    readMoreElement = wait.Until(d => {
                        var title = d.FindElements(activeSlideTitleLocator).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                        if (title == null) return null;
                        // search for any clickable link within the same slide container
                        var ancestor = title.FindElement(By.XPath("ancestor::div[contains(@class,'slider__slide')]"));
                        var link = ancestor.FindElements(By.TagName("a")).FirstOrDefault(a => a.Displayed && a.Enabled);
                        return link;
                    });
                }
                catch
                {
                    // leave readMoreElement as null and let the timeout below report
                }
            }

            // Capture href before clicking to avoid stale element when navigation occurs
            string href = readMoreElement.GetAttribute("href");
            readMoreElement.Click(); 
            return new ArticlePage(driver, href);
        }
    }
}
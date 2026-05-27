using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Keep this ONLY if you need it, otherwise remove
using System;
using System.Linq;
using System.Threading;
using Base.Utils;

namespace Locators.PageObjects
{
    public class InsightsPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private const string URL = "https://www.epam.com/insights";

        private readonly By nextButtonLocator = By.CssSelector("#main > div.content-container.parsys > div:nth-child(1) > div > div.slider__navigation > button.slider__right-arrow.slider-navigation-arrow");
        private readonly By activeSlideTitleLocator = By.ClassName("scaling-of-text-wrapper");
        private readonly By readMoreLocator = By.XPath("//*[@id=\"main\"]/div[1]/div[1]/div/div[1]/div[1]/div/div[4]/div/div/div/div[2]/a");
        private readonly By cookieBannerLocator = By.Id("onetrust-banner-sdk");

        public InsightsPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public InsightsPage Open()
        {
            driver.Navigate().GoToUrl(URL);
            // Attempt to dismiss cookie banner which may overlap interactive elements
            DismissCookieBanner();
            return this;
        }

        private void DismissCookieBanner()
        {
            try
            {
                var banners = driver.FindElements(cookieBannerLocator);
                var banner = banners.FirstOrDefault();
                if (banner == null) return;

                if (!banner.Displayed) return;

                // Try common accept/close buttons used by OneTrust
                var accept = banner.FindElements(By.Id("onetrust-accept-btn-handler")).FirstOrDefault();
                if (accept != null && accept.Displayed && accept.Enabled)
                {
                    accept.Click();
                    Thread.Sleep(300);
                    return;
                }

                var close = banner.FindElements(By.CssSelector(".onetrust-close-btn-handler, .onetrust-accept-btn-handler, button[title='Accept']")).FirstOrDefault();
                if (close != null && close.Displayed && close.Enabled)
                {
                    close.Click();
                    Thread.Sleep(300);
                    return;
                }

                // Fallback: hide banner via JS to avoid interception
                ((IJavaScriptExecutor)driver).ExecuteScript("var e=document.getElementById('onetrust-banner-sdk'); if(e) e.style.display='none';");
                Thread.Sleep(200);
            }
            catch
            {
                // ignore any errors during dismissal attempt
            }
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

                    Logger.Debug($"Found nextBtn: present={nextBtn != null}");
                                        if (nextBtn == null) Logger.Warn("Next button not found on carousel iteration " + i);

                    nextBtn.Click();


                // Дождаться появления элемента "Read more" (без присвоения read-only свойству)
                wait.Until(d => d.FindElement(readMoreLocator));
                                Logger.Debug("Waited for ReadMore element to appear");
                                                // additional info: number of links present
                                                try { Logger.Debug("ReadMoreCandidates=" + driver.FindElements(readMoreLocator).Count); } catch { }
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
                        var ancestor = title.FindElement(By.XPath("ancestor::div[contains(@class,'slider__slide')]") );
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
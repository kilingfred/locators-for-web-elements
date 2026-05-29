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
        private readonly By slideLocator =
    By.XPath("//div[contains(@class,'slider__slide')]");

        private readonly By activeSlideTitleLocator =
            By.XPath(".//*[contains(@class,'scaling-of-text-wrapper')]");

        private readonly By readMoreLocator =
            By.XPath(".//a[contains(.,'Read More')]");
        private readonly By cookieBannerLocator = By.Id("onetrust-banner-sdk");

        public InsightsPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public InsightsPage Open()
        {
            Logger.Info($"Navigating to {URL}");
            driver.Navigate().GoToUrl(URL);
            Logger.Debug("Page loaded, attempting to dismiss cookie banner if present");
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
                if (banner == null)
                {
                    Logger.Debug("No cookie banner found (onetrust)");
                    return;
                }

                Logger.Info("Cookie banner detected; attempting to dismiss");

                if (!banner.Displayed)
                {
                    Logger.Debug("Cookie banner present in DOM but not displayed");
                    return;
                }

                // Try common accept/close buttons used by OneTrust
                var accept = banner.FindElements(By.Id("onetrust-accept-btn-handler")).FirstOrDefault();
                if (accept != null && accept.Displayed && accept.Enabled)
                {
                    Logger.Info("Clicking OneTrust accept button");
                    accept.Click();
                    return;
                }

                var close = banner.FindElements(By.CssSelector(".onetrust-close-btn-handler, .onetrust-accept-btn-handler, button[title='Accept']")).FirstOrDefault();
                if (close != null && close.Displayed && close.Enabled)
                {
                    Logger.Info("Clicking cookie banner close/accept control");
                    close.Click();
                    return;
                }

                // Fallback: hide banner via JS to avoid interception
                Logger.Warn("Unable to find close button on cookie banner; hiding via JS");
                ((IJavaScriptExecutor)driver).ExecuteScript("var e=document.getElementById('onetrust-banner-sdk'); if(e) e.style.display='none';");
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to dismiss cookie banner: " + ex.Message);
            }
        }

        public void AdvanceCarousel(int times)
        {
            for (int i = 0; i < times; i++)
            {
                var nextBtn = wait.Until(d =>
                {
                    return d.FindElements(nextButtonLocator)
                        .FirstOrDefault(e =>
                            e.Displayed &&
                            e.Enabled);
                });

                nextBtn.Click();

                Thread.Sleep(1000);
            }
        }

        private IWebElement GetCurrentSlide()
        {
            return wait.Until(d =>
            {
                var slides = d.FindElements(slideLocator);

                // Prefer the slide that actually exposes a visible "Read More" link.
                // Using s.Text.Contains("Read More") is brittle because section headers
                // or other elements may contain that text. Check for a visible link
                // instead to reliably identify the active slide.
                foreach (var s in slides)
                {
                    try
                    {
                        if (!s.Displayed)
                            continue;

                        var readMoreLinks = s.FindElements(readMoreLocator);
                        if (readMoreLinks.Any(l => l.Displayed && l.Enabled))
                            return s;
                    }
                    catch
                    {
                        // ignore stale or detached elements
                    }
                }

                return null;
            });
        }

        public string GetActiveSlideTitle()
        {
            var slide = GetCurrentSlide();

            var title = slide
                .FindElement(activeSlideTitleLocator)
                .Text
                .Trim();

            Logger.Info($"Current slide title: {title}");

            return title;
        }

        public ArticlePage ClickReadMoreForActiveSlide()
        {
            var slide = GetCurrentSlide();

            var readMore = slide.FindElement(readMoreLocator);

            var href = readMore.GetAttribute("href");

            Logger.Info($"Opening article: {href}");

            ((IJavaScriptExecutor)driver)
                .ExecuteScript("arguments[0].click();", readMore);

            return new ArticlePage(driver, href);
        }
    }
}
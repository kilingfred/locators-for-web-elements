using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Base.Utils;

namespace Locators.PageObjects
{
    public class FooterPage
    {
        private readonly string URL = "https://www.epam.com/";
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        private readonly By footerLocator = By.TagName("footer");

        public FooterPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public FooterPage OpenHomePage()
        {
            driver.Navigate().GoToUrl(URL);
            return this;
        }

        public IWebElement FindCodeOfConductLink()
        {
            wait.Until(d => d.FindElements(footerLocator).Count > 0);
            var footer = driver.FindElements(footerLocator).FirstOrDefault();

            // Try several strategies
            var candidates = new By[] {
                By.PartialLinkText("Code of Ethical Conduct"),
                By.PartialLinkText("Code of Conduct"),
                By.XPath(".//a[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'code') and contains(., 'PDF')]")
            };

            foreach (var loc in candidates)
            {
                try
                {
                    var el = footer.FindElements(loc).FirstOrDefault(e => e.Displayed);
                    if (el != null) return el;
                }
                catch { }
            }

            // Last resort: search all links in footer
            try
            {
                var link = footer.FindElements(By.TagName("a")).FirstOrDefault(a => a.Text != null && a.Text.IndexOf("code", StringComparison.OrdinalIgnoreCase) >= 0 && a.Text.IndexOf("pdf", StringComparison.OrdinalIgnoreCase) >= 0);
                return link;
            }
            catch { }

            Logger.Warn("Code of conduct link not found in footer");
                        return null;
        }

        public void ScrollToFooter()
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        public void ClickLink(IWebElement link)
        {
            try { link.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", link); }
        }

        public void ClickCodeOfConductLink()
        {
            var link = FindCodeOfConductLink();
            if (link == null)
            {
                throw new NotFoundException("Code of conduct PDF link not found in footer");
            }

            try
            {
                link.Click();
            }
            catch
            {
                Logger.Warn("Direct click failed on link, falling back to JS click");
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", link);
            }
        }
    }
}
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Locators.PageObjects
{
    public class FooterPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        private By footerLocator = By.TagName("footer");

        public FooterPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public FooterPage OpenHomePage()
        {
            driver.Navigate().GoToUrl("https://www.epam.com/");
            return this;
        }

        public IWebElement FindCodeOfConductLink()
        {
            wait.Until(d => d.FindElements(footerLocator).Count > 0);
            var footer = driver.FindElements(footerLocator).FirstOrDefault();
            if (footer == null) return null;

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

        public bool WaitForFileCreatedAndStable(string path, TimeSpan timeout)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long lastSize = -1;
            while (sw.Elapsed < timeout)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var fi = new FileInfo(path);
                        if (fi.Length > 0 && fi.Length == lastSize)
                        {
                            return true;
                        }
                        lastSize = fi.Length;
                    }
                    catch { }
                }
                Thread.Sleep(500);
            }
            return false;
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
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", link);
            }
        }
    }
}
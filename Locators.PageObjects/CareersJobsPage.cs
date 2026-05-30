using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Locators.PageObjects
{
    public class CareersJobsPage
    {
        private const string URL = "https://careers.epam.com/en/jobs/ukraine";
        private IWebDriver webDriver;
        private WebDriverWait webDriverWait;

        // Locators
        private By searchInputLocator = By.CssSelector("form input[type='search'], form input[type='text'], form input");
        private By countryDropdownInputLocator = By.CssSelector("[data-testid='country-dropdown'] input[id^='react-select']");
        private By remoteCheckboxLocator = By.Name("vacancy_type-Remote");
        private By lastJobFromListLocator = By.XPath("(//div[contains(@class,'List_list')]/div)[last()]");
        private By expandButtonLocator = By.CssSelector("div.AccordionSection_header__kp8GP svg, div.JobCard_accordionHeader__UXZ0z svg, span > svg");
        private By requirementsLocator = By.TagName("li");

        public CareersJobsPage(IWebDriver driver)
        {
            this.webDriver = driver;
            this.webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
        }

        private IWebElement SearchInputElement => webDriverWait.Until(d => d.FindElement(searchInputLocator));
        private IWebElement LastJobFromListElement => webDriverWait.Until(d => d.FindElement(lastJobFromListLocator));

        private string SearchText { get; set; }

        public CareersJobsPage Open()
        {
            webDriver.Navigate().GoToUrl(URL);

            AcceptCookies();

            return this;
        }

        public CareersJobsPage InputSearchText(string searchText)
        {
            var el = this.SearchInputElement;

            // Wait until element is visible and enabled
            var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
            wait.Until(d =>
            {
                try { return el.Displayed && el.Enabled; }
                catch { return false; }
            });

            // Try to focus the input
            try { el.Click(); } catch { }
            try { el.Clear(); } catch { }

            try
            {
                el.SendKeys(searchText);
                // Ensure the search is applied by sending Enter; some React handlers trigger on Enter
                try { el.SendKeys(Keys.Enter); } catch { }
            }
            catch (ElementNotInteractableException)
            {
                // Fallback: set value via JS and trigger input event for React, then dispatch Enter key event
                ((IJavaScriptExecutor)webDriver).ExecuteScript(
                    "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input')); var ev = new KeyboardEvent('keydown', {key: 'Enter', keyCode: 13, which:13}); arguments[0].dispatchEvent(ev);",
                    el, searchText);
            }

            this.SearchText = searchText;
            return this;
        }

        public CareersJobsPage SelectCountryFromTheDropdownList(string country)
        {
            var input = webDriverWait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(
                        By.XPath("//div[@data-testid='country-dropdown']//input")
                    );

                    return (el.Displayed && el.Enabled)
                        ? el
                        : null;
                }
                catch
                {
                    return null;
                }
            });

            input.Click();

            // just type
            input.SendKeys(country);

            var option = webDriverWait.Until(d =>
            {
                return d.FindElements(By.XPath("//div[@role='option']"))
                    .FirstOrDefault(x =>
                        x.Text.Trim().Contains(
                            country,
                            StringComparison.OrdinalIgnoreCase));
            });

            if (option == null)
                throw new Exception($"Country '{country}' not found");

            ((IJavaScriptExecutor)webDriver)
                .ExecuteScript("arguments[0].click();", option);

            return this;
        }

        public CareersJobsPage SelectRemote()
        {
            By remoteLabelLocator = By.CssSelector("label[for^='checkbox-vacancy_type-Remote']");

            // 1. Wait, Scroll, and Click (Protected against StaleElementReferenceException)
            webDriverWait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(remoteLabelLocator);
                    if (el.Displayed && el.Enabled)
                    {
                        // Scroll into view to avoid header interception
                        ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);

                        try
                        {
                            el.Click();
                        }
                        catch (ElementClickInterceptedException)
                        {
                            ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", el);
                        }
                        return true;
                    }
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // If React rebuilds the DOM during the scroll/click, catch it and try again
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            // 2. Validate the State using the React data-checked attribute
            var validationWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
            validationWait.Until(d =>
            {
                try
                {
                    var hiddenInput = d.FindElement(remoteCheckboxLocator);
                    string dataChecked = hiddenInput.GetAttribute("data-checked");
                    return string.Equals(dataChecked, "true", StringComparison.OrdinalIgnoreCase);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            return this;
        }

        public CareersJobsPage ExpandLastJobFromList()
        {
            // Close cookie/consent banner if it overlays the page
            try
            {
                var closeSelectors = "#onetrust-banner-sdk .onetrust-close-btn-handler, .onetrust-close-btn-handler";
                var closeBtn = webDriver.FindElements(By.CssSelector(closeSelectors)).FirstOrDefault(b => b.Displayed);
                if (closeBtn != null)
                {
                    try { closeBtn.Click(); } catch { ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].click();", closeBtn); }
                    var waitBanner = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
                    waitBanner.Until(d => d.FindElements(By.Id("onetrust-banner-sdk")).All(e => !e.Displayed));
                }
            }
            catch (Exception ex) { Base.Utils.Logger.Warn("Cookie close attempt failed: " + ex.Message); }

            var listLocator = By.XPath("//div[contains(@class,'List_list')]/div");
            var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElements(listLocator).Count > 0);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    var items = webDriver.FindElements(listLocator);
                    if (items == null || items.Count == 0) throw new InvalidOperationException("No job items found");
                    var last = items.Last();

                    IWebElement btn = null;
                    try { btn = last.FindElement(expandButtonLocator); } catch { }
                    if (btn == null)
                    {
                        try { btn = last.FindElement(By.CssSelector("svg, button, [role='button']")); } catch { }
                    }

                    if (btn == null) throw new InvalidOperationException("Expand control not found inside the last job item.");

                    ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

                    try
                    {
                        btn.Click();
                        return this;
                    }
                    catch (Exception ex) when (ex is ElementClickInterceptedException || ex is ElementNotInteractableException)
                    {
                        ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].click();", btn);
                        return this;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    continue;
                }
            }

            throw new InvalidOperationException("Unable to expand last job after retries.");
        }

        public Boolean ContainsSearchedText()
        {
            bool textFound = false;
            ReadOnlyCollection<IWebElement> requirements = LastJobFromListElement.FindElements(requirementsLocator);
            foreach (IWebElement requirement in requirements)
            {
                IWebElement div = requirement.FindElement(By.TagName("div"));
                if (!string.IsNullOrEmpty(div.Text) && div.Text.IndexOf(this.SearchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    textFound = true;
                }
            }
            return textFound;
        }

    private void AcceptCookies()
        {
            try
            {
                var acceptButton = webDriver.FindElement(
                    By.XPath("//button[contains(.,'ACCEPT')]")
                );

                ((IJavaScriptExecutor)webDriver)
                    .ExecuteScript("arguments[0].click();", acceptButton);
            }
            catch
            {
                // already closed
            }
        }
    }
}
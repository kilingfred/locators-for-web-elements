using Base;
using Base.Utils;
using OpenQA.Selenium;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;

namespace Locators.Tests
{
    public partial class BaseTest
    {
        protected IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            // Clear log file before each test run to keep logs short and per-test
            Logger.Clear();

            // create or reuse driver via DriverSingleton; factory selection is handled in Base
            driver = DriverSingleton.GetDriver();
            Logger.Info("WebDriver instance created");
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                try
                {
                    try
                    {
                        driver.Quit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error during driver.Quit(): {ex.Message}", ex);
                    }

                    try
                    {
                        driver.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error during driver.Dispose(): {ex.Message}", ex);
                    }

                    DriverSingleton.CloseAndClear();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error during DriverSingleton.CloseAndClear(): {ex.Message}", ex);
                }
                finally
                {
                    driver = null;
                }
            }
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Setup();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            TearDown();
        }
    }
}

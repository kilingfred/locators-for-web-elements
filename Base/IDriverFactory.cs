using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace Base
{
    public interface IDriverFactory
    {
        string DownloadDirectory { get; }

        IWebDriver GetDriver();

        void CloseDriver(IWebDriver driver);
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class ArticlePage
{
    private IWebDriver driver;
    private WebDriverWait wait;
    // Use //h1 to find the element anywhere in the document
    private By pageHeader = By.TagName("h1");

    public ArticlePage(IWebDriver driver, string path)
    {
        this.driver = driver;
        this.Path = path;
        this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
    }

    private string Path { get; init; }

    // Use a lazy property that waits for visibility
    private IWebElement PageHeaderElement => wait.Until(d => d.FindElement(pageHeader));

    public ArticlePage Open()
    {
        driver.Navigate().GoToUrl(Path);
        return this;
    }

    public string GetArticleTitle()
    {
        return PageHeaderElement.Text.Trim();
    }
}
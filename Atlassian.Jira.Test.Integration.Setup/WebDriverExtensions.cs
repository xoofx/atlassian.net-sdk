using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Globalization;
using System.Linq;

namespace Atlassian.Jira.Test.Integration.Setup
{
    public static class WebDriverExtensions
    {
        public static bool UrlContains(this IWebDriver webDriver, string text)
        {
            var url = webDriver.Url;
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(url, text, CompareOptions.IgnoreCase) >= 0;
        }

        public static void WaitForUrlToContain(this IWebDriver webDriver, string text)
        {
            WaitForUrlToContain(webDriver, text, TimeSpan.FromMinutes(1));
        }

        public static void WaitForUrlToContain(this IWebDriver webDriver, string text, TimeSpan timeout)
        {
            var wait = new WebDriverWait(webDriver, timeout);
            wait.Until(wd => wd.UrlContains(text));
        }

        public static IWebElement WaitForElement(this IWebDriver webDriver, By locator)
        {
            return WaitForElement(webDriver, locator, TimeSpan.FromSeconds(10));
        }

        public static IWebElement WaitForElement(this IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            return WaitForElement(webDriver, timeout, wd => wd.FindElements(locator).FirstOrDefault());
        }

        public static IWebElement WaitForElement(this IWebDriver webDriver, Func<IWebDriver, IWebElement> func)
        {
            return WaitForElement(webDriver, TimeSpan.FromSeconds(10), func);
        }

        public static IWebElement WaitForElement(this IWebDriver webDriver, TimeSpan timeout, Func<IWebDriver, IWebElement> func)
        {
            var wait = new WebDriverWait(webDriver, timeout);
            wait.PollingInterval = TimeSpan.FromSeconds(1);
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            return wait.Until(func);
        }
    }
}

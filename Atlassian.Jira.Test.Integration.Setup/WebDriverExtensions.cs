using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira.Test.Integration.Setup
{
    public static class WebDriverExtensions
    {
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
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            return wait.Until(func);
        }
    }
}

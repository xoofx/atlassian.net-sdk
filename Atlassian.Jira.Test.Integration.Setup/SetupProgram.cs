using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Atlassian.Jira.Test.Integration.Setup
{
    class SetupProgram
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(typeof(SetupProgram).Assembly.Location);
            var arg = args.Length > 0 ? args[0].ToLowerInvariant() : null;
            var user = args.Length > 1 ? args[1].ToLowerInvariant() : null;
            var pass = args.Length > 2 ? args[2].ToLowerInvariant() : null;

            Environment.CurrentDirectory = currentDir;

            if (arg != null && arg.Equals("start", StringComparison.OrdinalIgnoreCase))
            {
                StartJira(currentDir);
            }
            else if (arg != null && arg.Equals("restore", StringComparison.OrdinalIgnoreCase))
            {
                SetupJira(currentDir, user, pass);
            }
            else
            {
                PrintInstructions();
            }

        }

        private static void PrintInstructions()
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("To setup JIRA to run integration tests:");
            Console.WriteLine("  1. 'JiraSetup.exe start'");
            Console.WriteLine("  2. Wait until tomcat container is fully ready.");
            Console.WriteLine("  3. Manually login to JIRA once and skip all the tutorials if needed.");
            Console.WriteLine("  4. 'JiraSetup.exe restore <user> <pass>'.");
            Console.WriteLine("  5. Wait until the back up restore is complete.");
            Console.WriteLine("-------------------------------------------------------");
        }

        private static void StartJira(string currentDir)
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Starting JIRA.");
            Console.WriteLine("Wait until the tomcat container is ready to accept requests.");
            Console.WriteLine("-------------------------------------------------------");

            var process = new Process();
            process.StartInfo.FileName = Path.Combine(currentDir, "StartJira.bat");
            process.Start();
        }

        private static void SetupJira(string currentDir, string user, string pass)
        {
            var webDriver = new ChromeDriver();

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Restoring test data.");
            Console.WriteLine("-------------------------------------------------------");

            // Login
            LoginToJira(webDriver, user, pass);

            // Restore TestData
            RestoreTestData(webDriver, currentDir);

            // Login again
            LoginToJira(webDriver, user, pass);

            // Install Jira software if necessary.
            InstallJiraSoftware(webDriver);

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("JIRA Restore Complete. You can now run the integration tests.");
            Console.WriteLine("-------------------------------------------------------");

            webDriver.Quit();
        }

        private static void RestoreTestData(ChromeDriver webDriver, string currentDir)
        {
            File.Copy(
                Path.Combine(currentDir, "TestData.zip"),
                Path.Combine(currentDir, @"amps-standalone-jira-7.1.7\target\jira\home\import\TestData.zip"),
                true);

            webDriver.Url = "http://localhost:2990/jira/secure/admin/XmlRestore!default.jspa";
            WaitForElement(webDriver, By.Name("filename")).SendKeys("TestData.zip");
            WaitForElement(webDriver, By.Id("restore-xml-data-backup-submit")).Click();

            // Wait until restore is complete
            WaitForElement(
                webDriver,
                TimeSpan.FromMinutes(10),
                wd => wd.FindElements(By.TagName("p"))
                    .FirstOrDefault(we => we.Text.Trim().Equals("Your import has been successful.", StringComparison.OrdinalIgnoreCase)));
        }

        private static Func<IWebDriver, IWebElement> FindJiraSectionFunc = (wd) =>
        {
            var sections = from section in wd.FindElements(By.ClassName("application-item"))
                           let name = section.FindElement(By.ClassName("application-name")).Text.Trim()
                           where name.Equals("Jira Software", StringComparison.OrdinalIgnoreCase)
                           select section;

            return sections.FirstOrDefault();
        };

        private static void InstallJiraSoftware(ChromeDriver webDriver)
        {
            webDriver.Url = "http://localhost:2990/jira/plugins/servlet/applications/versions-licenses";
            var jiraSection = WaitForElement(webDriver, FindJiraSectionFunc);
            var installButton = jiraSection.FindElements(By.ClassName("install-notification-action")).FirstOrDefault();

            if (installButton != null)
            {
                Console.WriteLine("Installing JIRA Software.");
                installButton.Click();

                var wait = new WebDriverWait(webDriver, TimeSpan.FromMinutes(10));
                wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                wait.Until(wd => !FindJiraSectionFunc(wd).FindElements(By.ClassName("install-notification-action")).Any());
            }
            else
            {
                Console.WriteLine("JIRA Software is already installed.");
            }
        }

        private static void LoginToJira(ChromeDriver webDriver, string user = "admin", string pass = "admin")
        {
            webDriver.Url = "http://localhost:2990/jira/login.jsp";
            WaitForElement(webDriver, By.Id("login-form-username")).SendKeys(user);
            WaitForElement(webDriver, By.Id("login-form-password")).SendKeys(pass);
            WaitForElement(webDriver, By.Id("login-form-submit")).Click();
            WaitForElement(webDriver, By.Id("header-details-user-fullname"), TimeSpan.FromSeconds(60));
        }

        private static IWebElement WaitForElement(IWebDriver webDriver, By locator)
        {
            return WaitForElement(webDriver, locator, TimeSpan.FromSeconds(10));
        }

        private static IWebElement WaitForElement(IWebDriver webDriver, By locator, TimeSpan timeout)
        {
            return WaitForElement(webDriver, timeout, wd => wd.FindElements(locator).FirstOrDefault());
        }

        private static IWebElement WaitForElement(IWebDriver webDriver, Func<IWebDriver, IWebElement> func)
        {
            return WaitForElement(webDriver, TimeSpan.FromSeconds(10), func);
        }

        private static IWebElement WaitForElement(IWebDriver webDriver, TimeSpan timeout, Func<IWebDriver, IWebElement> func)
        {
            var wait = new WebDriverWait(webDriver, timeout);
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            return wait.Until(func);
        }
    }
}

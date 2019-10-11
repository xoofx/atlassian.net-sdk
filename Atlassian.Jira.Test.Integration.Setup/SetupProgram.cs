using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Atlassian.Jira.Test.Integration.Setup
{
    public class SetupProgram
    {
        public const string URL = "http://localhost:8080";

        static void Main(string[] args)
        {
            WaitForJira().Wait();

            using (var webDriver = new ChromeDriver())
            {
                webDriver.Url = URL;

                SetupJira(webDriver);

                webDriver.Quit();
            };
        }

        private static async Task WaitForJira()
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                var retryCount = 0;

                do
                {
                    try
                    {
                        Console.Write($"Pinging server {URL}.");

                        retryCount++;
                        await Task.Delay(2000);
                        response = await client.GetAsync(URL);
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException)
                    {
                        Console.WriteLine($" Failed, retry count: {retryCount}");
                    }
                } while (retryCount < 30 && (response == null || response.StatusCode != HttpStatusCode.OK));

                Console.WriteLine($" Success!");
            }
        }

        private static void SetupJira(ChromeDriver webDriver)
        {
            Console.WriteLine("--- Starting to setup Jira ---");

            Console.WriteLine("Choose to manually setup jira.");
            webDriver.WaitForElement(By.XPath("//div[@data-choice-value='classic']"), TimeSpan.FromMinutes(5)).Click();

            Console.WriteLine("Click the next button.");
            webDriver.WaitForElement(By.Id("jira-setup-mode-submit")).Click();

            Console.WriteLine("Wait for database page, and click on the next button.");
            webDriver.WaitForElement(By.Id("jira-setup-database-submit")).Click();

            Console.WriteLine("Wait for the built-in database to be setup.");
            webDriver.WaitForElement(By.Id("jira-setupwizard-submit"), TimeSpan.FromMinutes(10));

            Console.WriteLine("Click on the import link.");
            webDriver.WaitForElement(By.TagName("a")).Click();

            Console.WriteLine("Wait for the import data page and import the test data.");
            webDriver.WaitForElement(By.Name("filename")).SendKeys("TestData.zip");
            webDriver.WaitForElement(By.Id("jira-setupwizard-outgoing-mailfalse")).Click();
            webDriver.WaitForElement(By.Id("jira-setupwizard-submit")).Click();

            Console.WriteLine("Wait until restore is complete.");
            webDriver.WaitForElement(By.Id("login-form-username"), TimeSpan.FromMinutes(10));

            Console.WriteLine("--- Finished setting up Jira ---");
        }
    }
}

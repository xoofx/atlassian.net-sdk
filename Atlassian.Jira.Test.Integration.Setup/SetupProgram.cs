using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Atlassian.Jira.Test.Integration.Setup
{
    public class SetupProgram
    {
        public const string URL = "http://localhost:8080";

        static void Main(string[] args)
        {
            WaitForJira().Wait();

            var chromeService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();
            options.LeaveBrowserRunning = true;
            options.AddArgument("no-sandbox");
            using (var webDriver = new ChromeDriver(chromeService, options, TimeSpan.FromMinutes(5)))
            {
                webDriver.Url = URL;

                try
                {
                    SetupJira(webDriver, args);
                    webDriver.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("-- Setup Failed. Browser will be kept running until you press a key. -- ");
                    Console.ResetColor();

                    Console.ReadKey();
                    webDriver.Quit();
                }
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
                } while (retryCount < 60 && (response == null || response.StatusCode != HttpStatusCode.OK));

                Console.WriteLine($" Success!");
            }
        }

        private static int GetStep(ChromeDriver webDriver)
        {
            if (webDriver.UrlContains("SetupMode"))
            {
                return 1;
            }
            else if (webDriver.UrlContains("SetupDatabase"))
            {
                return 2;
            }
            else if (webDriver.UrlContains("SetupApplicationProperties"))
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        private static void SetupJira(ChromeDriver webDriver, string[] args)
        {
            Console.WriteLine("--- Starting to setup Jira ---");
            webDriver.WaitForElement(By.Id("logo"), TimeSpan.FromMinutes(5));
            var step = GetStep(webDriver);

            if (step <= 1)
            {
                Console.WriteLine("Choose to manually setup jira.");
                webDriver.WaitForElement(By.XPath("//div[@data-choice-value='classic']"), TimeSpan.FromMinutes(5)).Click();

                Console.WriteLine("Click the next button.");
                webDriver.WaitForElement(By.Id("jira-setup-mode-submit")).Click();
            }

            if (step <= 2)
            {
                Console.WriteLine("Wait for database page, and click on the next button.");
                webDriver.WaitForElement(By.Id("jira-setup-database-submit")).Click();

                Console.WriteLine("Wait for the built-in database to be setup.");
                webDriver.WaitForElement(By.Id("jira-setupwizard-submit"), TimeSpan.FromMinutes(10));
            }

            if (step <= 3)
            {
                Console.WriteLine("Click on the import link.");
                webDriver.WaitForElement(By.TagName("a")).Click();
            }

            if (step <= 4)
            {
                var testDataFile = "TestData.zip";
                if (args != null && args.Length > 0 && !String.IsNullOrWhiteSpace(args[0]))
                {
                    testDataFile = $"TestData_{args[0]}.zip";
                }

                Console.WriteLine($"Wait for the import data page and import the test data. Using data file: {testDataFile}");

                webDriver.WaitForElement(By.Name("filename")).SendKeys(testDataFile);
                webDriver.WaitForElement(By.Id("jira-setupwizard-submit")).Click();

                Console.WriteLine("Wait until restore is complete (may take up to 20 minutes).");
                webDriver.WaitForElement(By.Id("login-form-username"), TimeSpan.FromMinutes(20));
            }

            Console.WriteLine("--- Finished setting up Jira ---");
        }
    }
}

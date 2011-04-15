using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Testing.Light;
using System.IO;
using System.Diagnostics;
using System.Threading;


namespace Atlassian.Jira.Test.Integration.Setup
{
    class SetupProgram
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(typeof(SetupProgram).Assembly.Location);
            Environment.CurrentDirectory = currentDir;

            if (args.Length > 0)
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "start":
                        StartJira(currentDir);
                        break;
                    case "setup":
                        SetupTestData(currentDir);
                        break;
                    default:
                        throw new ArgumentException(String.Format("Unknwon command '{0}'", args[0]));
                }
            }
            else
            {
                StartJira(currentDir);
                SetupTestData(currentDir);                
            }
        }

        private static bool IsJiraReady(int seconds)
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine(String.Format("Checking if JIRA is up (wait time: {0} seconds).", seconds));
            Console.WriteLine("-------------------------------------------------------");

            HtmlPage page = new HtmlPage(new Uri("http://localhost:2990/jira/"));

            try
            {
                page.Navigate("login.jsp");
                return page.Elements.Find("h2", 0).CachedInnerText.Trim().Equals("Welcome to Your Company JIRA", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
            }

            return false;
        }



        private static void SetupTestData(string currentDir)
        {
            int seconds = 0;
            while (!IsJiraReady(seconds))
            {
                Thread.Sleep(1000);
                seconds++;
            }

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Restoring test data.");
            Console.WriteLine("-------------------------------------------------------");


            HtmlPage page = new HtmlPage(new Uri("http://localhost:2990/jira/"));

            page.Navigate("login.jsp");

            // login
            page.Elements.Find("login-form-username").SetText("admin");
            page.Elements.Find("login-form-password").SetText("admin");
            page.Elements.Find("login").Click();

            // handle websudo
            page.Navigate("secure/admin/ViewApplicationProperties.jspa");
            page.Elements.Find("login-form-authenticatePassword").SetText("admin");
            page.Elements.Find("authenticateButton").Click();

            // go to configuration screen and set RPC on
            page.Navigate("secure/admin/EditApplicationProperties!default.jspa");
            page.Elements.Find("allowRpcOn", MatchMethod.Literal).Click();
            page.Elements.Find("edit_property").Click();

            // Restore TestData
            page.Navigate("secure/admin/XmlRestore!default.jspa");
            File.Copy(
                Path.Combine(currentDir, "TestData.zip"),
                Path.Combine(currentDir, @"amps-standalone\target\jira\home\import\TestData.zip"), 
                true);

            page.Elements.Find("filename", MatchMethod.Literal).SetText("TestData.zip");
            page.Elements.Find("restore_submit").Click();

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("JIRA Setup Complete.");
            Console.WriteLine("-------------------------------------------------------");
        }


        private static void StartJira(string currentDir)
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Starting JIRA.");
            Console.WriteLine("-------------------------------------------------------");

            var process = new Process();
            process.StartInfo.FileName = Path.Combine(currentDir, "StartJira.bat");
            process.Start();
        }

    }
}

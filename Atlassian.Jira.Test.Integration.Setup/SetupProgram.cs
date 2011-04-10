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
        private static bool _jiraStarted = false;
        private static Process _jira;
        private static string _currentDir;

        static void Main(string[] args)
        {
            _currentDir = Path.GetDirectoryName(typeof(SetupProgram).Assembly.Location);

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Starting JIRA.");
            Console.WriteLine("-------------------------------------------------------");
            
            _jira = StartJira();

            while (!IsJiraReady())
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Restoring test data.");
            Console.WriteLine("-------------------------------------------------------");

            SetupTestData();

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("JIRA Setup Complete.");
            Console.WriteLine("-------------------------------------------------------");

        }

        private static bool IsJiraReady()
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Checking if JIRA is up.");
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

      

        private static void SetupTestData()
        {
            var currentDir = typeof(SetupProgram).Assembly.Location;


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
                Path.Combine(_currentDir, "TestData.zip"), 
                Path.Combine(_currentDir, @"amps-standalone\target\jira\home\import\TestData.zip"), 
                true);

            page.Elements.Find("filename", MatchMethod.Literal).SetText("TestData.zip");
            page.Elements.Find("restore_submit").Click();
        }


        private static Process StartJira()
        {
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(_currentDir, "StartJira.bat");
            process.Start();

            return process;
        }

    }
}

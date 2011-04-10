using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Testing.Light;


namespace Atlassian.Jira.Test.Integration.Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Seting up JIRA.");
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

            // add a project
            page.Navigate("secure/admin/AddProject!default.jspa");
            page.Elements.Find("name", MatchMethod.Literal).SetText("Test Project");
            page.Elements.Find("key", MatchMethod.Literal).SetText("TST");
            page.Elements.Find("lead", MatchMethod.Literal).SetText("admin");
            page.Elements.Find("add_submit").Click();


            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("JIRA Setup Complete.");
            Console.WriteLine("-------------------------------------------------------");

        }
    }
}

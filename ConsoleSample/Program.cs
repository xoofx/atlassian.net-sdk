using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira;
using Atlassian.Jira.Linq;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var jira = new Jira("http://localhost:8080", "admin", "admin");

            var issue = new Issue() 
            { 
                Summary = "Sample issue", 
                Project = "TST",
                Type = "1"
            };

            jira.CreateIssue(issue);

            Console.ReadKey();
        }
    }
}

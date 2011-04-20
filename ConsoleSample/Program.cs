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
            var jira = new Jira("http://localhost:2990/streams", "admin", "admin");

            var issue = (from i in jira.IssueSearch()
                         where i.Key == "ONE-1"
                         select i).ToArray()[0];

            issue.Summary += "(Updated)";

            jira.UpdateIssue(issue);

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}

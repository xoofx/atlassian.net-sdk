using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var jira = new JiraInstance(
               "http://localhost:8080/rpc/soap/jirasoapservice-v2",
               "admin",
               "admin");

            jira.Debug = true;

            var issues = from i in jira.IssueSearch()
                         where i.Status == "In Progress"
                         select i;

            foreach (var i in issues)
            {
                Console.WriteLine(i.Summary);
            }

            Console.ReadKey();
        }
    }
}

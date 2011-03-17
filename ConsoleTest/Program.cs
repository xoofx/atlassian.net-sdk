using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira;
using Atlassian.Jira.Linq;

namespace ConsoleTest
{
    class Program
    {
       

        static void Main(string[] args)
        {
            var jira = new JiraInstance(
                "http://localhost:8080/rpc/soap/jirasoapservice-v2",
                "admin",
                "admin");

            var issues = from i in jira.IssueSearch()
                          where i.Assignee == "admin"
                          select i;

            foreach (var i in issues)
            {
                Console.WriteLine(i.Summary);
            }

            Console.ReadKey();
        }
    }
}

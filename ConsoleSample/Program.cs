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
            var jira = new Jira("http://jira.atlassian.com");

            jira.Debug = true;

            var issues = from i in jira.IssueSearch()
                         where i.Summary == "custom field"
                         orderby i.Created, i.DueDate descending
                         select i;

            foreach (var i in issues)
            {
                Console.WriteLine(i.Summary);
            }

            Console.ReadKey();
        }
    }
}

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
            
            var translator = new JqlExpressionTranslator();

            var jira = new Jira(translator, new TestRemoteService());

            //var jira = new JiraInstance(
            //   "http://localhost:8080",
            //   "admin",
            //   "admin");

            jira.Debug = true;

            var issues = from i in jira.IssueSearch()
                         where i.Summary == "foo"
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

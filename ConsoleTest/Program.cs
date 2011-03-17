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
            var translator = new JqlExpressionTranslator();
            var remoteService = new JiraRemoteService();
            var provider = new JiraQueryProvider(translator, remoteService);

            var jira = new JiraInstance(provider);

            var issues = from i in jira.IssueSearch()
                          where i.Summary == "first bug"
                          select i;

            foreach (var i in issues)
            {
                Console.WriteLine(i.Summary);
            }
        }
    }
}

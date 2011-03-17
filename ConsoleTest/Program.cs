using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace ConsoleTest
{
    class Program
    {
        private static JqlExpressionTranslator _translator;

        private static JiraInstance CreateJiraInstance()
        {
            _translator = new JqlExpressionTranslator();
            var provider = new JiraQueryProvider(_translator);

            return new JiraInstance(provider);

        }

        static void Main(string[] args)
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary == null
                          select i).ToArray();

            
        }
    }
}

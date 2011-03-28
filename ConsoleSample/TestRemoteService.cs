using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;
using Atlassian.Jira;

namespace ConsoleSample
{
    class TestRemoteService : IJiraSoapServiceClient
    {
        public IList<Atlassian.Jira.Issue> GetIssuesFromJql(string p)
        {
            return new List<Issue>();
        }

        public string Login(string username, string password)
        {
            return "";
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults)
        {
            return new RemoteIssue[0];
        }
    }
}

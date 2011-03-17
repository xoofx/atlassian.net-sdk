using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;
using Atlassian.Jira;

namespace ConsoleSample
{
    class TestRemoteService: IJiraRemoteService
    {
        public IList<Atlassian.Jira.Issue> GetIssuesFromJql(string p)
        {
            return new List<Issue>();
        }
    }
}

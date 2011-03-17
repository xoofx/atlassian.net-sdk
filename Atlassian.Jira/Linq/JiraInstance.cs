using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    public class JiraInstance
    {
        private JiraQueryProvider _provider;

        public JiraInstance(JiraQueryProvider provider)
        {
            this._provider = provider;
        }

        public JiraQueryable<Issue> IssueSearch()
        {
            return new JiraQueryable<Issue>(_provider);
        }
    }
}

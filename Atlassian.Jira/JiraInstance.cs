using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    public class JiraInstance
    {
        private readonly JiraQueryProvider _provider;
        
        public JiraInstance(string url, string username, string password)
        {
            this._provider = new JiraQueryProvider(
                new JqlExpressionTranslator(),
                new JiraRemoteService(new Uri(url), username, password));
        }

        public JiraInstance(JiraQueryProvider provider)
        {
            this._provider = provider;
        }

        public JiraQueryable<Issue> IssueSearch()
        {
            return new JiraQueryable<Issue>(_provider);
        }

        public bool Debug
        {
            get
            {
                return _provider.Debug;
            }
            set
            {
                _provider.Debug = value;
            }
        }

    }
}

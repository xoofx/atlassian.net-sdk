using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA server
    /// </summary>
    public class Jira
    {
        private readonly JiraQueryProvider _provider;
        
        /// <summary>
        /// Create a connection to a JIRA server
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        /// <param name="username">username to use to authenticate</param>
        /// <param name="password">passowrd to use to authenticate</param>
        public Jira(string url, string username, string password)
        {
            this._provider = new JiraQueryProvider(
                new JqlExpressionTranslator(),
                new JiraRemoteService(new Uri(url), username, password));
        }

        /// <summary>
        /// Create a connection to a JIRA server
        /// </summary>
        /// <param name="translator">The JqlExpressionTranslation to use</param>
        /// <param name="remoteService">The RemoteService implementation to use</param>
        public Jira(IJqlExpressionTranslator translator, IJiraRemoteService remoteService)
        {
            this._provider = new JiraQueryProvider(translator, remoteService);
        }

        /// <summary>
        /// Query the issues database
        /// </summary>
        /// <returns>IQueryable of Issue</returns>
        public JiraQueryable<Issue> IssueSearch()
        {
            return new JiraQueryable<Issue>(_provider);
        }

        /// <summary>
        /// Whether to print the translated JQL to console
        /// </summary>
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

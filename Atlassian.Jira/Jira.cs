using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;
using System.ServiceModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA server
    /// </summary>
    public class Jira
    {
        private readonly JiraQueryProvider _provider;

        private readonly IJiraSoapServiceClient _jiraSoapService;
        private readonly string _username = null;
        private readonly string _password = null;
        private string _token = String.Empty;

        /// <summary>
        /// Create a connection to a JIRA server with anonymous access
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        public Jira(string url)
            : this(url, null, null)
        {
        }

        /// <summary>
        /// Create a connection to a JIRA server with provided credentials
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        /// <param name="username">username to use to authenticate</param>
        /// <param name="password">passowrd to use to authenticate</param>
        public Jira(string url, string username, string password)
            :this(new JiraSoapServiceClientWrapper(url), username, password)
        {
        }

        /// <summary>
        /// Create a connection to a JIRA server using provided JiraSoapService implementation and provided credentials
        /// </summary>
        /// <param name="jiraSoapService">Soap client proxy that can connect to Jira instance</param>
        /// <param name="username">username to use to authenticate</param>
        /// <param name="password">passowrd to use to authenticate</param>
        public Jira(IJiraSoapServiceClient jiraSoapService, string username, string password)
            : this(new JqlExpressionTranslator(), jiraSoapService, username, password)
        {
        }

        /// <summary>
        /// Create a connection to a JIRA server using the provided translator and jiraserver
        /// </summary>
        /// <param name="translator">The JqlExpressionTranslation to use</param>
        /// <param name="jiraSoapService">Soap client proxy that can connect to Jira instance</param>
        /// <param name="username">username to use to authenticate</param>
        /// <param name="password">passowrd to use to authenticate</param>
        public Jira(IJqlExpressionTranslator translator, IJiraSoapServiceClient jiraSoapService, string username, string password)
        {
            _username = username;
            _password = password;
            _jiraSoapService = jiraSoapService;

            this._provider = new JiraQueryProvider(translator, this);
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
        public bool Debug { get; set; }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <returns>List of Issues that match the search query</returns>
        public IList<Issue> GetIssuesFromJql(string jql)
        {
            if (this.Debug)
            {
                Console.WriteLine("JQL: " + jql);
            }

            if (_username != null && _password != null && String.IsNullOrEmpty(_token))
            {
                _token = _jiraSoapService.Login(_username, _password);
            }

            IList<Issue> issues = new List<Issue>();
            foreach (RemoteIssue i in _jiraSoapService.GetIssuesFromJqlSearch(_token, jql, 20))
            {
                issues.Add(new Issue()
                {
                    Summary = i.summary,
                    Assignee = i.assignee,
                    Description = i.description,
                    Environment = i.environment,
                    Key = new ComparableTextField(i.key),
                    Priority = new ComparableTextField(i.priority),
                    Project = i.project,
                    Reporter = i.reporter,
                    Resolution = new ComparableTextField(i.resolution),
                    Status = i.status,
                    Type = i.type,
                    Votes = i.votes.Value
                });
            }

            return issues;
        }
    }
}

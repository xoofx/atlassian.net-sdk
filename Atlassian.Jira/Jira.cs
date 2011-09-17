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
        private const int DEFAULT_MAX_ISSUES_PER_REQUEST = 20;

        private readonly JiraQueryProvider _provider;

        private readonly IJiraSoapServiceClient _jiraSoapService;
        private readonly IFileSystem _fileSystem;
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
            :this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  username, 
                  password)
        {
        }

        /// <summary>
        /// Create a connection to a JIRA server
        /// </summary>
        public Jira(IJqlExpressionVisitor translator, 
                    IJiraSoapServiceClient jiraSoapService, 
                    IFileSystem fileSystem,
                    string username, 
                    string password)
        {
            _username = username;
            _password = password;
            _jiraSoapService = jiraSoapService;
            _fileSystem = fileSystem;
            this.MaxIssuesPerRequest = DEFAULT_MAX_ISSUES_PER_REQUEST;

            this._provider = new JiraQueryProvider(translator, this);
        }

        /// <summary>
        /// Whether to print the translated JQL to console
        /// </summary>
        public bool Debug 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Maximum number of issues per request
        /// </summary>
        public int MaxIssuesPerRequest { get; set; }

        /// <summary>
        /// Url to the JIRA server
        /// </summary>
        public string Url
        {
            get { return _jiraSoapService.Url; }
        }

        internal string UserName
        {
            get { return _username; }
        }

        internal string Password
        {
            get { return _password; }
        }

        internal IFileSystem FileSystem
        {
            get { return _fileSystem; }
        } 

        /// <summary>
        /// Query the issues database
        /// </summary>
        /// <returns>IQueryable of Issue</returns>
        public JiraQueryable<Issue> Issues
        {
            get
            {
                return new JiraQueryable<Issue>(_provider);
            }
        }
        
        private string GetAuthenticationToken()
        {
            if (!String.IsNullOrEmpty(_username) 
                && !String.IsNullOrEmpty(_password) && String.IsNullOrEmpty(_token))
            {
                _token = _jiraSoapService.Login(_username, _password);
            }

            return _token;
        }

        /// <summary>
        /// Gets an issue from the JIRA server
        /// </summary>
        /// <param name="key">The key of the issue</param>
        /// <returns></returns>
        public Issue GetIssue(string key)
        {
            return (from i in Issues
                    where i.Key == key
                    select i).First();
        }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <returns>Collection of Issues that match the search query</returns>
        public IEnumerable<Issue> GetIssuesFromJql(string jql)
        {
            return GetIssuesFromJql(jql, null);
        }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return</param>
        /// <returns>List of Issues that match the search query</returns>
        public IList<Issue> GetIssuesFromJql(string jql, int? maxIssues)
        {
            if (this.Debug)
            {
                Console.WriteLine("JQL: " + jql);
            }

            var token = GetAuthenticationToken();

            IList<Issue> issues = new List<Issue>();

            foreach (RemoteIssue remoteIssue in _jiraSoapService.GetIssuesFromJqlSearch(token, jql, maxIssues?? MaxIssuesPerRequest))
            {
                issues.Add(new Issue(this, remoteIssue));
            }

            return issues;
        }

        /// <summary>
        /// Create a new issue on the server
        /// </summary>
        /// <param name="newIssue">New Issue to create</param>
        /// <returns>Created issue with values propulated from server</returns>
        public Issue CreateIssue(Issue newIssue)
        {
            var token = GetAuthenticationToken();

            var remoteIssue = newIssue.ToRemote();
            
            remoteIssue = _jiraSoapService.CreateIssue(_token, remoteIssue);

            return new Issue(this, remoteIssue);
        }

        /// <summary>
        /// Updates an issue
        /// </summary>
        /// <param name="issue">Issue to update</param>
        /// <returns>Updated issues with values populated from server</returns>
        public Issue UpdateIssue(Issue issue)
        {
            var token = GetAuthenticationToken();

            var fields = ((IRemoteIssueFieldProvider)issue).GetRemoteFields();

            var remoteIssue = _jiraSoapService.UpdateIssue(token, issue.Key.Value, fields);

            return new Issue(this, remoteIssue);
        }
    

        /// <summary>
        /// Returns all the issue types within JIRA
        /// </summary>
        /// <param name="projectKey">If provided, returns issue types only for given project</param>
        /// <returns>Collection of JIRA issue types</returns>
        public IEnumerable<JiraNamedEntity> GetIssueTypes(string projectKey = null)
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetIssueTypes(token, projectKey).Select(t => new JiraNamedEntity(t));
        }

        /// <summary>
        /// Returns all versions defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the versions from</param>
        /// <returns>Collection of JIRA versions.</returns>
        public IEnumerable<Version> GetVersions(string projectKey)
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetVersions(token, projectKey).Select(v => new Version(v)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue priorities</returns>
        public IEnumerable<JiraNamedEntity> GetIssuePriorities()
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetPriorities(token).Select(p => new JiraNamedEntity(p));
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue statuses</returns>
        public IEnumerable<JiraNamedEntity> GetIssueStatuses()
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetStatuses(token).Select(s => new JiraNamedEntity(s));
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue resolutions</returns>
        public IEnumerable<JiraNamedEntity> GetIssueResolutions()
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetResolutions(token).Select(r => new JiraNamedEntity(r));
        }

        internal IList<Attachment> GetAttachmentsForIssue(string issueKey)
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.GetAttachmentsFromIssue(token, issueKey).Select(a => new Attachment(this, new WebClientWrapper(), a)).ToList();
        }

        internal bool AddAttachmentsToIssue(string issueKey, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            var token = GetAuthenticationToken();
            return _jiraSoapService.AddBase64EncodedAttachmentsToIssue(token, issueKey, fileNames, base64EncodedAttachmentData);
        }

        internal IList<Comment> GetCommentsForIssue(string issueKey)
        {
            var token = GetAuthenticationToken();

            return _jiraSoapService.GetCommentsFromIssue(token, issueKey).Select(c => new Comment(c)).ToList();
        }

        internal void AddCommentToIssue(string issueKey, Comment comment)
        {
            var token = GetAuthenticationToken();

            _jiraSoapService.AddComment(token, issueKey, comment.toRemote());
        }
    }
}

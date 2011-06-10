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
            :this(new JqlExpressionTranslator(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  username, 
                  password)
        {
        }

        /// <summary>
        /// Create a connection to a JIRA server
        /// </summary>
        public Jira(IJqlExpressionTranslator translator, 
                    IJiraSoapServiceClient jiraSoapService, 
                    IFileSystem fileSystem,
                    string username, 
                    string password)
        {
            _username = username;
            _password = password;
            _jiraSoapService = jiraSoapService;
            _fileSystem = fileSystem;

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

            var token = GetAuthenticationToken();

            IList<Issue> issues = new List<Issue>();
            foreach (RemoteIssue remoteIssue in _jiraSoapService.GetIssuesFromJqlSearch(token, jql, 20))
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

            var fields = issue.GetUpdatedFields();

            var remoteIssue = _jiraSoapService.UpdateIssue(token, issue.Key.Value, fields);

            return new Issue(this, remoteIssue);
        }

        internal IList<Attachment> GetAttachmentsForIssue(string issueKey)
        {
            var token = GetAuthenticationToken();

            var attachments = new List<Attachment>();
            foreach (var remoteAttachment in _jiraSoapService.GetAttachmentsFromIssue(token, issueKey))
            {
                attachments.Add(new Attachment(this, new WebClientWrapper(), remoteAttachment));
            }

            return attachments;
        }

        internal bool AddAttachmentsToIssue(string issueKey, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            var token = GetAuthenticationToken();

            return _jiraSoapService.AddBase64EncodedAttachmentsToIssue(token, issueKey, fileNames, base64EncodedAttachmentData);
        }

        internal IList<Comment> GetCommentsForIssue(string issueKey)
        {
            var token = GetAuthenticationToken();

            var comments = new List<Comment>();
            foreach(var remote in _jiraSoapService.GetCommentsFromIssue(token, issueKey))
            {
                comments.Add(new Comment(remote));
            }

            return comments;
        }

        internal void AddCommentToIssue(string issueKey, Comment comment)
        {
            var token = GetAuthenticationToken();

            _jiraSoapService.AddComment(token, issueKey, comment.toRemote());
        }
    }
}

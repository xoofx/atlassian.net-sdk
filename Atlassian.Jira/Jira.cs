using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Util.DoubleKeyDictionary;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA server
    /// </summary>
    public class Jira
    {
        internal const string DEFAULT_DATE_FORMAT = "yyyy/MM/dd";
        internal static CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-us");

        private const int DEFAULT_MAX_ISSUES_PER_REQUEST = 20;
        private const string ALL_PROJECTS_KEY = "[ALL_PROJECTS]";
        private const string REMOTE_AUTH_EXCEPTION_STRING = "com.atlassian.jira.rpc.exception.RemoteAuthenticationException";

        private readonly JiraQueryProvider _provider;
        private readonly IJiraServiceClient _jiraService;
        private readonly IFileSystem _fileSystem;
        private readonly IJiraRestClient _restClient;
        private readonly JiraCredentials _credentials;

        private string _token = String.Empty;
        private Dictionary<string, IEnumerable<ProjectVersion>> _cachedVersions = new Dictionary<string, IEnumerable<ProjectVersion>>();
        private Dictionary<string, IEnumerable<ProjectComponent>> _cachedComponents = new Dictionary<string, IEnumerable<ProjectComponent>>();
        private Dictionary<string, IEnumerable<IssueType>> _cachedIssueTypes = new Dictionary<string, IEnumerable<IssueType>>();
        private Dictionary<string, IEnumerable<IssueType>> _cachedSubTaskIssueTypes = new Dictionary<string, IEnumerable<IssueType>>();
        private Dictionary<string, Issue> _cachedIssues = new Dictionary<string, Issue>();
        private IEnumerable<JiraNamedEntity> _cachedFilters = null;
        private IEnumerable<IssuePriority> _cachedPriorities = null;
        private IEnumerable<IssueStatus> _cachedStatuses = null;
        private IEnumerable<IssueResolution> _cachedResolutions = null;
        private IEnumerable<Project> _cachedProjects = null;

        private IEnumerable<CustomField> _cachedCustomFields = null;
        private Dictionary<string, IEnumerable<JiraNamedEntity>> _cachedFieldsForEdit = new Dictionary<string, IEnumerable<JiraNamedEntity>>();
        private DoubleKeyDictionary<string, string, IEnumerable<JiraNamedEntity>> _cachedFieldsForAction = new DoubleKeyDictionary<string, string, IEnumerable<JiraNamedEntity>>();

        /// <summary>
        /// Create a proxy that connects with a JIRA server with anonymous access.
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        public Jira(string url)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem())
        {
        }

        /// <summary>
        /// Create a proxy that connects with a JIRA server with specified credentials.
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        /// <param name="username">Username used to authenticate</param>
        /// <param name="password">Password used to authenticate</param>
        public Jira(string url, string username, string password)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  new JiraCredentials(username, password))
        {
        }

        /// <summary>
        /// Create a proxy that connects with a JIRA server with specified access token.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="token">JIRA access token to use.</param>
        /// <param name="credentialsProvider">Provider of credentials needed to re-generate token.</param>
        [Obsolete]
        public Jira(string url, string token, Func<JiraCredentials> credentialsProvider = null)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  credentialsProvider == null ? new JiraCredentials(null) : credentialsProvider(),
                  token)
        {
        }

        /// <summary>
        /// Create a proxy that connects with a JIRA server with specified access token.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="token">JIRA access token to use.</param>
        /// <param name="credentials">Credentials used to re-generate token.</param>
        public Jira(string url, string token, JiraCredentials credentials)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  credentials,
                  token)
        {
        }

        /// <summary>
        /// Create a proxy that connects with a JIRA server with specified access token and dependencies.
        /// </summary>
        public Jira(IJqlExpressionVisitor translator,
                    IJiraServiceClient jiraService,
                    IFileSystem fileSystem,
                    JiraCredentials credentials = null,
                    string accessToken = null)
        {
            _provider = new JiraQueryProvider(translator, this);
            _jiraService = jiraService;
            _fileSystem = fileSystem;
            _token = accessToken;
            _credentials = credentials;
            _restClient = jiraService as IJiraRestClient;

            this.MaxIssuesPerRequest = DEFAULT_MAX_ISSUES_PER_REQUEST;
            this.Debug = false;

            if (_restClient == null && !String.IsNullOrEmpty(jiraService.Url))
            {
                if (this._credentials == null)
                {
                    this._restClient = new JiraRestServiceClient(jiraService.Url);
                }
                else
                {
                    this._restClient = new JiraRestServiceClient(jiraService.Url, _credentials.UserName, _credentials.Password);
                }
            }
        }

        /// <summary>
        /// Creates a JIRA client configured to use the REST API.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="username">Username used to authenticate.</param>
        /// <param name="password">Password used to authenticate.</param>
        /// <param name="settings">Settings to configure the rest client.</param>
        /// <returns>Jira object configured to use REST API.</returns>
        public static Jira CreateRestClient(string url, string username = null, string password = null, JiraRestClientSettings settings = null)
        {
            settings = settings ?? new JiraRestClientSettings();
            var restClient = new JiraRestServiceClient(url, username, password, settings);

            return new Jira(
                new JqlExpressionVisitor(),
                restClient,
                new FileSystem(),
                new JiraCredentials(username, password));
        }

        private bool IsAnonymous
        {
            get
            {
                return this._credentials == null;
            }
        }

        internal IJiraServiceClient RemoteService
        {
            get
            {
                return _jiraService;
            }
        }

        /// <summary>
        /// Gets a client configured to interact with JIRA's REST API.
        /// </summary>
        public IJiraRestClient RestClient
        {
            get { return this._restClient; }
        }

        /// <summary>
        /// Whether to print the translated JQL to console
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Maximum number of issues per request
        /// </summary>
        public int MaxIssuesPerRequest { get; set; }

        /// <summary>
        /// Url to the JIRA server
        /// </summary>
        public string Url
        {
            get { return _jiraService.Url; }
        }

        internal JiraCredentials GetCredentials()
        {
            if (this._credentials == null)
            {
                throw new InvalidOperationException("Unable to get user and password, credentials has not been set.");
            }

            return this._credentials;
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
        /// Returns issues that match the specified filter
        /// </summary>
        /// <param name="filterName">The name of the filter used for the search</param>
        /// <param name="start">The place in the result set to use as the first issue returned</param>
        /// <param name="maxResults">The maximum number of issues to return</param>
        public IEnumerable<Issue> GetIssuesFromFilter(string filterName, int start = 0, int? maxResults = null)
        {
            var filter = this.GetFilters().FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

            if (filter == null)
            {
                throw new InvalidOperationException(String.Format("Filter with name '{0}' was not found", filterName));
            }

            return WithToken(token =>
            {
                return _jiraService.GetIssuesFromFilterWithLimit(token, filter.Id, start, maxResults ?? this.MaxIssuesPerRequest).Select(i => new Issue(this, i));
            });
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
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <returns>Collection of Issues that match the search query</returns>
        public IEnumerable<Issue> GetIssuesFromJql(string jql, int? maxIssues = null, int startAt = 0)
        {
            if (this.Debug)
            {
                Trace.WriteLine("JQL: " + jql);
            }

            IList<Issue> issues = new List<Issue>();

            WithToken(token =>
            {
                foreach (RemoteIssue remoteIssue in _jiraService.GetIssuesFromJqlSearch(token, jql, maxIssues ?? MaxIssuesPerRequest, startAt))
                {
                    issues.Add(new Issue(this, remoteIssue));
                }
            });

            return issues;
        }

        /// <summary>
        /// Returns a new issue that when saved will be created on the remote JIRA server
        /// </summary>
        public Issue CreateIssue(string project, string parentIssueKey = null)
        {
            return new Issue(this, project, parentIssueKey);
        }

        /// <summary>
        /// Deletes the issue specified from the JIRA server.
        /// </summary>
        /// <param name="issue">Issue to delete.</param>
        public void DeleteIssue(Issue issue)
        {
            if (issue.Key == null || String.IsNullOrEmpty(issue.Key.ToString()))
            {
                throw new InvalidOperationException("Unable to delete issue, it has not been created.");
            }

            WithToken(token =>
            {
                _jiraService.DeleteIssue(token, issue.Key.ToString());
            });
        }

        /// <summary>
        /// Returns all the sub-tasks issue types within JIRA.
        /// </summary>
        public IEnumerable<IssueType> GetSubTaskIssueTypes()
        {
            if (!_cachedSubTaskIssueTypes.ContainsKey(ALL_PROJECTS_KEY))
            {
                WithToken(token =>
                {
                    _cachedSubTaskIssueTypes.Add(
                        ALL_PROJECTS_KEY,
                        _jiraService.GetSubTaskIssueTypes(token).Select(remoteIssueType => new IssueType(remoteIssueType)));
                });
            }

            return _cachedSubTaskIssueTypes[ALL_PROJECTS_KEY];
        }

        /// <summary>
        /// Returns all the issue types within JIRA
        /// </summary>
        /// <param name="projectKey">If provided, returns issue types only for given project</param>
        /// <returns>Collection of JIRA issue types</returns>
        public IEnumerable<IssueType> GetIssueTypes(string projectKey = null)
        {
            string projectId = null;
            if (projectKey != null)
            {
                var project = this.GetProjects().FirstOrDefault(p => p.Key.Equals(projectKey, StringComparison.OrdinalIgnoreCase));
                if (project != null)
                {
                    projectId = project.Id;
                }
            }

            projectKey = projectKey ?? ALL_PROJECTS_KEY;

            if (!_cachedIssueTypes.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    _cachedIssueTypes.Add(projectKey, _jiraService.GetIssueTypes(token, projectId).Select(remoteIssueType => new IssueType(remoteIssueType)));
                });
            }

            return _cachedIssueTypes[projectKey];
        }

        /// <summary>
        /// Returns all versions defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the versions from</param>
        /// <returns>Collection of JIRA versions.</returns>
        public IEnumerable<ProjectVersion> GetProjectVersions(string projectKey)
        {
            if (!_cachedVersions.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    _cachedVersions.Add(projectKey, _jiraService.GetVersions(token, projectKey).Select(v => new ProjectVersion(v)));
                });
            }

            return _cachedVersions[projectKey];
        }

        /// <summary>
        /// Returns all components defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the components from</param>
        /// <returns>Collection of JIRA components</returns>
        public IEnumerable<ProjectComponent> GetProjectComponents(string projectKey)
        {
            if (!_cachedComponents.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    _cachedComponents.Add(projectKey, _jiraService.GetComponents(token, projectKey).Select(c => new ProjectComponent(c)));
                });
            }

            return _cachedComponents[projectKey];
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue priorities</returns>
        public IEnumerable<IssuePriority> GetIssuePriorities()
        {
            if (_cachedPriorities == null)
            {
                WithToken(token =>
                {
                    _cachedPriorities = _jiraService.GetPriorities(token).Select(p => new IssuePriority(p));
                });
            }

            return _cachedPriorities;
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue statuses</returns>
        public IEnumerable<IssueStatus> GetIssueStatuses()
        {
            if (_cachedStatuses == null)
            {
                WithToken(token =>
                {
                    _cachedStatuses = _jiraService.GetStatuses(token).Select(s => new IssueStatus(s));
                });
            }

            return _cachedStatuses;
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue resolutions</returns>
        public IEnumerable<IssueResolution> GetIssueResolutions()
        {
            if (_cachedResolutions == null)
            {
                WithToken(token =>
                {
                    _cachedResolutions = _jiraService.GetResolutions(token).Select(r => new IssueResolution(r));
                });
            }

            return _cachedResolutions;
        }

        /// <summary>
        /// Returns all custom fields within JIRA
        /// </summary>
        /// <returns>Collection of JIRA custom fields</returns>
        public IEnumerable<CustomField> GetCustomFields()
        {
            if (_cachedCustomFields == null)
            {
                WithToken(token =>
                {
                    _cachedCustomFields = _jiraService.GetCustomFields(token).Select(f => new CustomField(f));
                });
            }
            return _cachedCustomFields;
        }

        /// <summary>
        /// Returns all custom fields within JIRA.
        /// </summary>
        /// <returns>Collection of JIRA custom fields</returns>
        public Task<IEnumerable<CustomField>> GetCustomFieldsAsync()
        {
            if (this.RestClient == null)
            {
                throw new NotSupportedException("This method is only supported with REST provider.");
            }

            return this.RestClient.GetCustomFieldsAsync();
        }

        /// <summary>
        /// Returns the favourite filters for the user
        /// </summary>
        public IEnumerable<JiraNamedEntity> GetFilters()
        {
            if (_cachedFilters == null)
            {
                WithToken(token =>
                {
                    _cachedFilters = _jiraService.GetFavouriteFilters(token).Select(f => new JiraNamedEntity(f));
                });
            }

            return _cachedFilters;
        }

        /// <summary>
        /// Returns all projects defined in JIRA
        /// </summary>
        public IEnumerable<Project> GetProjects()
        {
            if (_cachedProjects == null)
            {
                WithToken(token =>
                {
                    _cachedProjects = _jiraService.GetProjects(token).Select(p => new Project(p));
                });
            }

            return _cachedProjects;
        }

        /// <summary>
        /// Executes an action using the user's authentication token.
        /// </summary>
        /// <remarks>
        /// If action fails with 'com.atlassian.jira.rpc.exception.RemoteAuthenticationException'
        /// a new token will be requested from server and the action called again.
        /// </remarks>
        public void WithToken(Action<string> action)
        {
            WithToken<object>(t =>
            {
                action(t);
                return null;
            });
        }

        /// <summary>
        /// Executes an action using the user's authentication token and the jira soap client
        /// </summary>
        /// <remarks>
        /// If action fails with 'com.atlassian.jira.rpc.exception.RemoteAuthenticationException'
        /// a new token will be requested from server and the action called again.
        /// </remarks>
        public void WithToken(Action<string, IJiraServiceClient> action)
        {
            WithToken<object>((token, client) =>
            {
                action(token, client);
                return null;
            });
        }

        /// <summary>
        /// Executes a function using the user's authentication token.
        /// </summary>
        /// <remarks>
        /// If function fails with 'com.atlassian.jira.rpc.exception.RemoteAuthenticationException'
        /// a new token will be requested from server and the function called again.
        /// </remarks>
        public TResult WithToken<TResult>(Func<string, TResult> function)
        {
            return WithToken((token, client) => function(token));
        }

        /// <summary>
        /// Executes a function using the user's authentication token and the jira soap client
        /// </summary>
        /// <remarks>
        /// If function fails with 'com.atlassian.jira.rpc.exception.RemoteAuthenticationException'
        /// a new token will be requested from server and the function called again.
        /// </remarks>
        public TResult WithToken<TResult>(Func<string, IJiraServiceClient, TResult> function)
        {
            if (!IsAnonymous && String.IsNullOrEmpty(_token))
            {
                _token = GetAccessToken();
            }

            try
            {
                return function(_token, this.RemoteService);
            }
            catch (FaultException fe)
            {
                if (IsAnonymous
                    || fe.Message.IndexOf(REMOTE_AUTH_EXCEPTION_STRING, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw;
                }

                _token = GetAccessToken();
                return function(_token, this.RemoteService);
            }
        }

        /// <summary>
        /// Retrieves an access token from server using current credentials.
        /// </summary>
        public string GetAccessToken()
        {
            var credentials = GetCredentials();
            return _jiraService.Login(credentials.UserName, credentials.Password);
        }

        internal IEnumerable<JiraNamedEntity> GetFieldsForAction(Issue issue, string actionId)
        {
            if (issue.Key == null)
            {
                issue = GetOneIssueFromProject(issue.Project);
            }

            if (!_cachedFieldsForAction.ContainsKey(issue.Project, actionId))
            {
                WithToken((token, service) =>
                {
                    _cachedFieldsForAction.Add(issue.Project, actionId, _jiraService.GetFieldsForAction(token, issue.Key.Value, actionId)
                        .Select(f => new JiraNamedEntity(f)));
                });
            }

            return _cachedFieldsForAction[issue.Project, actionId];
        }

        internal IEnumerable<JiraNamedEntity> GetFieldsForEdit(Issue issue)
        {
            if (issue.Key == null)
            {
                issue = GetOneIssueFromProject(issue.Project);
            }

            if (!_cachedFieldsForEdit.ContainsKey(issue.Project))
            {
                WithToken(token =>
                {
                    _cachedFieldsForEdit.Add(issue.Project, _jiraService.GetFieldsForEdit(token, issue.Key.Value)
                        .Select(f => new JiraNamedEntity(f)));
                });
            }

            return _cachedFieldsForEdit[issue.Project];
        }

        private Issue GetOneIssueFromProject(string projectKey)
        {
            if (!this._cachedIssues.ContainsKey(projectKey))
            {
                var tempIssue = this.GetIssuesFromJql(String.Format("project = \"{0}\"", projectKey), 1)
                                .FirstOrDefault();

                if (tempIssue == null)
                {
                    throw new InvalidOperationException("Project must contain at least one issue to be able to retrieve issue fields.");
                }

                this._cachedIssues.Add(projectKey, tempIssue);
            }

            return this._cachedIssues[projectKey];
        }
    }
}

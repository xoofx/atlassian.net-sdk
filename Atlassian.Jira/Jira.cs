using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
        internal const string ALL_PROJECTS_KEY = "[ALL_PROJECTS]";
        internal static CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-us");

        private const int DEFAULT_MAX_ISSUES_PER_REQUEST = 20;
        private const string REMOTE_AUTH_EXCEPTION_STRING = "com.atlassian.jira.rpc.exception.RemoteAuthenticationException";

        private readonly JiraQueryProvider _provider;
        private readonly IJiraSoapClient _jiraService;
        private readonly IFileSystem _fileSystem;
        private readonly IJiraRestClient _restClient;
        private readonly JiraCredentials _credentials;
        private readonly JiraCache _cache;

        private string _token = String.Empty;
        private Dictionary<string, Issue> _cachedIssues = new Dictionary<string, Issue>();
        private Dictionary<string, IEnumerable<JiraNamedEntity>> _cachedFieldsForEdit = new Dictionary<string, IEnumerable<JiraNamedEntity>>();
        private DoubleKeyDictionary<string, string, IEnumerable<JiraNamedEntity>> _cachedFieldsForAction = new DoubleKeyDictionary<string, string, IEnumerable<JiraNamedEntity>>();
        private IEnumerable<JiraNamedEntity> _cachedFilters = null;

        /// <summary>
        /// Create a SOAP client that connects with a JIRA server with anonymous access.
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        [Obsolete("Use Jira.CreateSoapClient or Jira.CreateRestClient instead.", true)]
        public Jira(string url)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem())
        {
        }

        /// <summary>
        /// Create a SOAP client that connects with a JIRA server with specified credentials.
        /// </summary>
        /// <param name="url">Url to the JIRA server</param>
        /// <param name="username">Username used to authenticate</param>
        /// <param name="password">Password used to authenticate</param>
        [Obsolete("Use Jira.CreateSoapClient or Jira.CreateRestClient instead.", true)]
        public Jira(string url, string username, string password)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  new JiraCredentials(username, password))
        {
        }

        /// <summary>
        /// Create a SOAP client that connects with a JIRA server with specified access token.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="token">JIRA access token to use.</param>
        /// <param name="credentialsProvider">Provider of credentials needed to re-generate token.</param>
        [Obsolete("Use Jira.CreateSoapClient or Jira.CreateRestClient instead.", true)]
        public Jira(string url, string token, Func<JiraCredentials> credentialsProvider = null)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  credentialsProvider == null ? new JiraCredentials(null) : credentialsProvider(),
                  token)
        {
        }

        /// <summary>
        /// Create a SOAP client that connects with a JIRA server with specified access token.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="token">JIRA access token to use.</param>
        /// <param name="credentials">Credentials used to re-generate token.</param>
        [Obsolete("Use Jira.CreateSoapClient or Jira.CreateRestClient instead.", true)]
        public Jira(string url, string token, JiraCredentials credentials)
            : this(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  credentials,
                  token)
        {
        }

        /// <summary>
        /// Create a client that connects with a JIRA server with specified dependencies.
        /// </summary>
        public Jira(IJqlExpressionVisitor translator,
                    IJiraSoapClient jiraService,
                    IFileSystem fileSystem,
                    JiraCredentials credentials = null,
                    string accessToken = null,
                    JiraCache cache = null)
        {
            _provider = new JiraQueryProvider(translator, this);
            _jiraService = jiraService;
            _fileSystem = fileSystem;
            _token = accessToken;
            _credentials = credentials;
            _restClient = jiraService as IJiraRestClient;
            _cache = cache ?? new JiraCache();

            this.MaxIssuesPerRequest = DEFAULT_MAX_ISSUES_PER_REQUEST;
            this.Debug = false;

            if (_restClient == null && !String.IsNullOrEmpty(jiraService.Url))
            {
                var options = new JiraRestClient.Options()
                {
                    Url = jiraService.Url,
                    RestClientSettings = new JiraRestClientSettings(),
                    GetCurrentJiraFunc = () => this
                };

                if (this._credentials != null)
                {
                    options.Username = _credentials.UserName;
                    options.Password = _credentials.Password;
                }

                this._restClient = new JiraRestClient(options);
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
            Jira jira = null;
            var options = new JiraRestClient.Options()
            {
                Url = url,
                Username = username,
                Password = password,
                RestClientSettings = settings ?? new JiraRestClientSettings(),
                GetCurrentJiraFunc = () => jira
            };

            var restClient = new JiraRestClient(options);
            jira = CreateRestClient(restClient, new JiraCredentials(username, password), options.RestClientSettings.Cache);

            return jira;
        }

        /// <summary>
        /// Creates a JIRA client with service dependency.
        /// </summary>
        public static Jira CreateRestClient(IJiraClient jiraClient, JiraCredentials credentials = null, JiraCache cache = null)
        {
            return new Jira(
                new JqlExpressionVisitor(),
                jiraClient,
                new FileSystem(),
                credentials,
                null,
                cache);
        }

        /// <summary>
        /// Creates a JIRA client configured to use the SOAP API.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="username">Username used to authenticate.</param>
        /// <param name="password">Password used to authenticate.</param>
        [Obsolete("SOAP API has been deprecated and removed from JIRA, migrate to use the REST API instead.")]
        public static Jira CreateSoapClient(string url, string username = null, string password = null)
        {
            return new Jira(new JqlExpressionVisitor(),
                  new JiraSoapServiceClientWrapper(url),
                  new FileSystem(),
                  new JiraCredentials(username, password));
        }

        /// <summary>
        /// Creates a JIRA client configured to use the SOAP API with specified access token.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="token">JIRA access token to use.</param>
        /// <param name="credentials">Credentials used to re-generate token.</param>
        [Obsolete("SOAP API has been deprecated and removed from JIRA, migrate to use the REST API instead.")]
        public static Jira CreateSoapClient(string url, string token, JiraCredentials credentials = null)
        {
            return new Jira(new JqlExpressionVisitor(),
                new JiraSoapServiceClientWrapper(url),
                new FileSystem(),
                credentials,
                token);
        }

        private bool IsAnonymous
        {
            get
            {
                return this._credentials == null;
            }
        }

        internal IJiraSoapClient RemoteService
        {
            get
            {
                return _jiraService;
            }
        }

        /// <summary>
        /// Gets the cache for frequently retrieved server items from JIRA.
        /// </summary>
        public JiraCache Cache
        {
            get
            {
                return _cache;
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
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues, int startAt, CancellationToken token)
        {
            return this.RestClient.GetIssuesFromJqlAsync(jql, maxIssues, startAt, token);
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
            if (!_cache.SubTaskIssueTypes.ContainsKey(ALL_PROJECTS_KEY))
            {
                WithToken(token =>
                {
                    var results = _jiraService.GetSubTaskIssueTypes(token).Select(remoteIssueType => new IssueType(remoteIssueType));
                    _cache.SubTaskIssueTypes.AddIfMIssing(new JiraEntityDictionary<IssueType>(ALL_PROJECTS_KEY, results));
                });
            }

            return _cache.SubTaskIssueTypes[ALL_PROJECTS_KEY].Values;
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

            if (!_cache.IssueTypes.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    var result = _jiraService.GetIssueTypes(token, projectId).Select(remoteIssueType => new IssueType(remoteIssueType));
                    _cache.IssueTypes.AddIfMIssing(new JiraEntityDictionary<IssueType>(projectKey, result));
                });
            }

            return _cache.IssueTypes[projectKey].Values;
        }

        /// <summary>
        /// Returns all the issue types within JIRA.
        /// </summary>
        public Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token)
        {
            return this.RestClient.GetIssueTypesAsync(token);
        }

        /// <summary>
        /// Returns all versions defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the versions from</param>
        /// <returns>Collection of JIRA versions.</returns>
        public IEnumerable<ProjectVersion> GetProjectVersions(string projectKey)
        {
            if (!_cache.Versions.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    var results = _jiraService.GetVersions(token, projectKey).Select(v => new ProjectVersion(this, v));
                    _cache.Versions.AddIfMIssing(new JiraEntityDictionary<ProjectVersion>(projectKey, results));
                });
            }

            return _cache.Versions[projectKey].Values;
        }

        /// <summary>
        /// Returns all components defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the components from</param>
        /// <returns>Collection of JIRA components</returns>
        public IEnumerable<ProjectComponent> GetProjectComponents(string projectKey)
        {
            if (!_cache.Components.ContainsKey(projectKey))
            {
                WithToken(token =>
                {
                    var results = _jiraService.GetComponents(token, projectKey).Select(c => new ProjectComponent(c));
                    _cache.Components.AddIfMIssing(new JiraEntityDictionary<ProjectComponent>(projectKey, results));
                });
            }

            return _cache.Components[projectKey].Values;
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue priorities</returns>
        public IEnumerable<IssuePriority> GetIssuePriorities()
        {
            if (!_cache.Priorities.Any())
            {
                WithToken(token =>
                {
                    _cache.Priorities.AddIfMIssing(_jiraService.GetPriorities(token).Select(p => new IssuePriority(p)));
                });
            }

            return _cache.Priorities.Values;
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA.
        /// </summary>
        public Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync(CancellationToken token)
        {
            return this.RestClient.GetIssuePrioritiesAsync(token);
        }

        /// <summary>
        /// Returns all available issue link types.
        /// </summary>
        public IEnumerable<IssueLinkType> GetIssueLinkTypes()
        {
            try
            {
                return this.GetIssueLinkTypesAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Returns all available issue link types.
        /// </summary>
        public Task<IEnumerable<IssueLinkType>> GetIssueLinkTypesAsync(CancellationToken token)
        {
            return this.RestClient.GetIssueLinkTypesAsync(token);
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue statuses</returns>
        public IEnumerable<IssueStatus> GetIssueStatuses()
        {
            if (!_cache.Statuses.Any())
            {
                WithToken(token =>
                {
                    _cache.Statuses.AddIfMIssing(_jiraService.GetStatuses(token).Select(s => new IssueStatus(s)));
                });
            }

            return _cache.Statuses.Values;
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA.
        /// </summary>
        public Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(CancellationToken token)
        {
            return this.RestClient.GetIssueStatusesAsync(token);
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue resolutions</returns>
        public IEnumerable<IssueResolution> GetIssueResolutions()
        {
            if (!_cache.Resolutions.Any())
            {
                WithToken(token =>
                {
                    _cache.Resolutions.AddIfMIssing(_jiraService.GetResolutions(token).Select(r => new IssueResolution(r)));
                });
            }

            return _cache.Resolutions.Values;
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        public Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync(CancellationToken token)
        {
            return this.RestClient.GetIssueResolutionsAsync(token);
        }

        /// <summary>
        /// Returns all custom fields within JIRA
        /// </summary>
        /// <returns>Collection of JIRA custom fields</returns>
        public IEnumerable<CustomField> GetCustomFields()
        {
            if (!_cache.CustomFields.Any())
            {
                WithToken(token =>
                {
                    _cache.CustomFields.AddIfMIssing(_jiraService.GetCustomFields(token).Select(f => new CustomField(f)));
                });
            }
            return _cache.CustomFields.Values;
        }

        /// <summary>
        /// Returns all custom fields within JIRA.
        /// </summary>
        public Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken token)
        {
            return this.RestClient.GetCustomFieldsAsync(token);
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
        /// Returns all projects defined in JIRA.
        /// </summary>
        public IEnumerable<Project> GetProjects()
        {
            if (!_cache.Projects.Any())
            {
                WithToken(token =>
                {
                    _cache.Projects.AddIfMIssing(_jiraService.GetProjects(token).Select(p => new Project(this, p)));
                });
            }

            return _cache.Projects.Values;
        }

        /// <summary>
        /// Returns all projects defined in JIRA.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken token)
        {
            return this.RestClient.GetProjectsAsync(token);
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
        public void WithToken(Action<string, IJiraSoapClient> action)
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
        public TResult WithToken<TResult>(Func<string, IJiraSoapClient, TResult> function)
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

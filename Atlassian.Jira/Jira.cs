using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly JiraCredentials _credentials;
        private readonly JiraCache _cache;
        private readonly ServiceLocator _services;

        /// <summary>
        /// Create a client that connects with a JIRA server with specified dependencies.
        /// </summary>
        public Jira(ServiceLocator services, JiraCredentials credentials = null, JiraCache cache = null)
        {
            _services = services;
            _credentials = credentials;
            _cache = cache ?? new JiraCache();

            this.MaxIssuesPerRequest = DEFAULT_MAX_ISSUES_PER_REQUEST;
            this.Debug = false;
        }

        /// <summary>
        /// Creates a JIRA rest client.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="username">Username used to authenticate.</param>
        /// <param name="password">Password used to authenticate.</param>
        /// <param name="settings">Settings to configure the rest client.</param>
        /// <returns>Jira object configured to use REST API.</returns>
        public static Jira CreateRestClient(string url, string username = null, string password = null, JiraRestClientSettings settings = null)
        {
            var services = new ServiceLocator();
            settings = settings ?? new JiraRestClientSettings();
            var jira = new Jira(services, new JiraCredentials(username, password), settings.Cache);
            var restClient = new JiraRestClient(services, url, username, password, settings);

            ConfigureDefaultServices(services, jira, restClient);

            return jira;
        }

        /// <summary>
        /// Creates a JIRA client with the given rest client implementation.
        /// </summary>
        /// <param name="jiraClient">Rest client to use.</param>
        /// <param name="credentials">Credentials to use.</param>
        /// <param name="cache">Cache to use.</param>
        public static Jira CreateRestClient(IJiraRestClient jiraClient = null, JiraCredentials credentials = null, JiraCache cache = null)
        {
            var services = new ServiceLocator();
            var jira = new Jira(services, credentials, cache);
            ConfigureDefaultServices(services, jira, jiraClient);
            return jira;
        }

        /// <summary>
        /// Gets the service locator for this jira instance.
        /// </summary>
        public ServiceLocator Services
        {
            get
            {
                return _services;
            }
        }

        /// <summary>
        /// Gets an object to interact with the projects of jira.
        /// </summary>
        public IProjectService Projects
        {
            get
            {
                return Services.Get<IProjectService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the users of jira.
        /// </summary>
        public IJiraUserService Users
        {
            get
            {
                return Services.Get<IJiraUserService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the user groups of jira.
        /// </summary>
        public IJiraGroupService Groups
        {
            get
            {
                return Services.Get<IJiraGroupService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue of jira.
        /// </summary>
        public IIssueService Issues
        {
            get
            {
                return Services.Get<IIssueService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue fields of jira.
        /// </summary>
        public IIssueFieldService Fields
        {
            get
            {
                return Services.Get<IIssueFieldService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue filters of jira.
        /// </summary>
        public IIssueFilterService Filters
        {
            get
            {
                return Services.Get<IIssueFilterService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue priorities of jira.
        /// </summary>
        public IIssuePriorityService Priorities
        {
            get
            {
                return Services.Get<IIssuePriorityService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue resolutions of jira.
        /// </summary>
        public IIssueResolutionService Resolutions
        {
            get
            {
                return Services.Get<IIssueResolutionService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue statuses of jira.
        /// </summary>
        public IIssueStatusService Statuses
        {
            get
            {
                return Services.Get<IIssueStatusService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue link types of jira.
        /// </summary>
        public IIssueLinkService Links
        {
            get
            {
                return Services.Get<IIssueLinkService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the issue types of jira.
        /// </summary>
        public IIssueTypeService IssueTypes
        {
            get
            {
                return Services.Get<IIssueTypeService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the project versions of jira.
        /// </summary>
        public IProjectVersionService Versions
        {
            get
            {
                return Services.Get<IProjectVersionService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the project components of jira.
        /// </summary>
        public IProjectComponentService Components
        {
            get
            {
                return Services.Get<IProjectComponentService>();
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
            get
            {
                return Services.Get<IJiraRestClient>();
            }
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
            get { return RestClient.Url; }
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
            get
            {
                return Services.Get<IFileSystem>();
            }
        }

        /// <summary>
        /// Gets an issue from the JIRA server
        /// </summary>
        /// <param name="key">The key of the issue</param>
        [Obsolete("Use Jira.Issues instead.")]
        public Issue GetIssue(string key)
        {
            return this.Issues.GetIssueAsync(key).Result;
        }

        /// <summary>
        /// Returns issues that match the specified filter
        /// </summary>
        /// <param name="filterName">The name of the filter used for the search</param>
        /// <param name="start">The place in the result set to use as the first issue returned</param>
        /// <param name="maxResults">The maximum number of issues to return</param>
        [Obsolete("Use Filters.GetIssuesFromFavoriteAsync instead.")]
        public IEnumerable<Issue> GetIssuesFromFilter(string filterName, int start = 0, int? maxResults = null)
        {
            return Filters.GetIssuesFromFavoriteAsync(filterName, maxResults, start).Result;
        }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <returns>Collection of Issues that match the search query</returns>
        [Obsolete("Use Jira.Issues instead.")]
        public IEnumerable<Issue> GetIssuesFromJql(string jql)
        {
            return GetIssuesFromJqlAsync(jql).Result;
        }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <returns>Collection of Issues that match the search query</returns>
        [Obsolete("Use Jira.Issues instead.")]
        public IEnumerable<Issue> GetIssuesFromJql(string jql, int? maxIssues = null, int startAt = 0)
        {
            return GetIssuesFromJqlAsync(jql, maxIssues, startAt).Result;
        }

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        [Obsolete("Use Jira.Issues instead.")]
        public Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues = null, int startAt = 0, CancellationToken token = default(CancellationToken))
        {
            return this.Issues.GetIsssuesFromJqlAsync(jql, maxIssues, startAt, token);
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
        [Obsolete("Use Jira.Issues instead.")]
        public void DeleteIssue(Issue issue)
        {
            Issues.DeleteIssueAsync(issue.OriginalRemoteIssue.key).Wait();
        }

        /// <summary>
        /// Returns all the sub-tasks issue types within JIRA.
        /// </summary>
        [Obsolete("Use Jira.IssueTypes instead.")]
        public IEnumerable<IssueType> GetSubTaskIssueTypes()
        {
            return IssueTypes.GetIssueTypesAsync().Result.Where(t => t.IsSubTask);
        }

        /// <summary>
        /// Returns all the issue types within JIRA
        /// </summary>
        /// <param name="projectKey">If provided, returns issue types only for given project</param>
        /// <returns>Collection of JIRA issue types</returns>
        [Obsolete("Use Jira.IssueTypes instead.")]
        public IEnumerable<IssueType> GetIssueTypes(string projectKey = null)
        {
            return IssueTypes.GetIssueTypesAsync().Result;
        }

        /// <summary>
        /// Returns all the issue types within JIRA.
        /// </summary>
        [Obsolete("Use Jira.IssueTypes instead.")]
        public Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token = default(CancellationToken))
        {
            return IssueTypes.GetIssueTypesAsync(token);
        }

        /// <summary>
        /// Returns all versions defined on a JIRA project
        /// </summary>
        /// <param name="projectKey">The project to retrieve the versions from</param>
        /// <returns>Collection of JIRA versions.</returns>
        [Obsolete("Use Jira.ProjectVersions instead.")]
        public IEnumerable<ProjectVersion> GetProjectVersions(string projectKey)
        {
            return Versions.GetVersionsAsync(projectKey).Result;
        }

        /// <summary>
        /// Returns all components defined on a JIRA project.
        /// </summary>
        /// <param name="projectKey">The project to retrieve the components from.</param>
        /// <returns>Collection of JIRA components</returns>
        [Obsolete("Use Jira.ProjectComponents instead.")]
        public IEnumerable<ProjectComponent> GetProjectComponents(string projectKey)
        {
            return Components.GetComponentsAsync(projectKey).Result;
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue priorities</returns>
        [Obsolete("Please use Jira.IssuePriorities instead")]
        public IEnumerable<IssuePriority> GetIssuePriorities()
        {
            return GetIssuePrioritiesAsync().Result;
        }

        /// <summary>
        /// Returns all the issue priorities within JIRA.
        /// </summary>
        [Obsolete("Please use Jira.IssuePriorities instead")]
        public Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync(CancellationToken token = default(CancellationToken))
        {
            return this.Priorities.GetPrioritiesAsync(token);
        }

        /// <summary>
        /// Returns all available issue link types.
        /// </summary>
        [Obsolete("Use Jira.IssueLinkTypes instead.")]
        public IEnumerable<IssueLinkType> GetIssueLinkTypes()
        {
            return GetIssueLinkTypesAsync().Result;
        }

        /// <summary>
        /// Returns all available issue link types.
        /// </summary>
        [Obsolete("Use Jira.IssueLinkTypes instead.")]
        public Task<IEnumerable<IssueLinkType>> GetIssueLinkTypesAsync(CancellationToken token = default(CancellationToken))
        {
            return Links.GetLinkTypesAsync(token);
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue statuses</returns>
        [Obsolete("Use Jira.IssueStatuses instead.")]
        public IEnumerable<IssueStatus> GetIssueStatuses()
        {
            return this.GetIssueStatusesAsync().Result;
        }

        /// <summary>
        /// Returns all the issue statuses within JIRA.
        /// </summary>
        [Obsolete("Use Jira.IssueStatuses instead.")]
        public Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(CancellationToken token = default(CancellationToken))
        {
            return this.Statuses.GetStatusesAsync(token);
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        /// <returns>Collection of JIRA issue resolutions</returns>
        [Obsolete("Use Jira.IssueResolutions instead.")]
        public IEnumerable<IssueResolution> GetIssueResolutions()
        {
            return GetIssueResolutionsAsync().Result;
        }

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        [Obsolete("Use Jira.IssueResolutions instead.")]
        public Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync(CancellationToken token = default(CancellationToken))
        {
            return this.Resolutions.GetResolutionsAsync(token);
        }

        /// <summary>
        /// Returns all custom fields within JIRA
        /// </summary>
        /// <returns>Collection of JIRA custom fields</returns>
        [Obsolete("Use Jira.Fields.GetCustomFieldsAsync instead.")]
        public IEnumerable<CustomField> GetCustomFields()
        {
            return Fields.GetCustomFieldsAsync().Result;
        }

        /// <summary>
        /// Returns all custom fields within JIRA.
        /// </summary>
        [Obsolete("Use Jira.Fields.GetCustomFieldsAsync instead.")]
        public Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken token)
        {
            return Fields.GetCustomFieldsAsync(token);
        }

        /// <summary>
        /// Returns the favourite filters for the user
        /// </summary>
        [Obsolete("Use Jira.IssueFilters instead.")]
        public IEnumerable<JiraNamedEntity> GetFilters()
        {
            return Filters.GetFavouritesAsync().Result.Cast<JiraNamedEntity>();
        }

        /// <summary>
        /// Returns all projects defined in JIRA.
        /// </summary>
        [Obsolete("Use Jira.Projects instead.")]
        public IEnumerable<Project> GetProjects()
        {
            return Projects.GetProjectsAsync().Result;
        }

        /// <summary>
        /// Returns user by username.
        /// </summary>
        /// <param name="userName">The username of the user to get.</param>
        /// <param name="token">Cancelation token for this operation.</param>
        /// <exception cref="ArgumentException">ArgumentException is thrown if passed username is null or empty string.</exception>
        [Obsolete("Use Jira.Users instead.")]
        public Task<JiraUser> GetUserAsync(string userName, CancellationToken token = default(CancellationToken))
        {
            return Users.GetUserAsync(userName, token);
        }

        /// <summary>
        /// Returns all projects defined in JIRA.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        [Obsolete("Use Jira.Projects instead.")]
        public Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken token)
        {
            return this.Projects.GetProjectsAsync(token);
        }

        private static void ConfigureDefaultServices(ServiceLocator services, Jira jira, IJiraRestClient restClient)
        {
            services.Register<IProjectVersionService>(() => new ProjectVersionService(jira));
            services.Register<IProjectComponentService>(() => new ProjectComponentService(jira));
            services.Register<IIssuePriorityService>(() => new IssuePriorityService(jira));
            services.Register<IIssueResolutionService>(() => new IssueResolutionService(jira));
            services.Register<IIssueStatusService>(() => new IssueStatusService(jira));
            services.Register<IIssueLinkService>(() => new IssueLinkService(jira));
            services.Register<IIssueTypeService>(() => new IssueTypeService(jira));
            services.Register<IIssueFilterService>(() => new IssueFilterService(jira));
            services.Register<IIssueFieldService>(() => new IssueFieldService(jira));
            services.Register<IIssueService>(() => new IssueService(jira));
            services.Register<IJiraUserService>(() => new JiraUserService(jira));
            services.Register<IJiraGroupService>(() => new JiraGroupService(jira));
            services.Register<IProjectService>(() => new ProjectService(jira));
            services.Register<IJqlExpressionVisitor>(() => new JqlExpressionVisitor());
            services.Register<IFileSystem>(() => new FileSystem());
            services.Register(() => restClient);
        }
    }
}

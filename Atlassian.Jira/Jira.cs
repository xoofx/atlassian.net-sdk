using System;
using System.Globalization;
using Atlassian.Jira.Linq;
using Atlassian.Jira.OAuth;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a JIRA server
    /// </summary>
    public class Jira
    {
        internal const string DEFAULT_DATE_FORMAT = "yyyy/MM/dd";
        internal const string DEFAULT_DATE_TIME_FORMAT = DEFAULT_DATE_FORMAT + " HH:mm";
        internal static CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-us");

        private readonly JiraCache _cache;
        private readonly ServiceLocator _services;

        /// <summary>
        /// Create a client that connects with a JIRA server with specified dependencies.
        /// </summary>
        public Jira(ServiceLocator services, JiraCache cache = null)
        {
            _services = services;
            _cache = cache ?? new JiraCache();

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
            settings = settings ?? new JiraRestClientSettings();
            var restClient = new JiraRestClient(url, username, password, settings);

            return CreateRestClient(restClient, settings.Cache);
        }

        /// <summary>
        /// Creates a JIRA rest client using OAuth authentication protocol.
        /// </summary>
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
        /// <param name="consumerSecret">The consumer public key in XML format.</param>
        /// <param name="oAuthAccessToken">The access token provided by Jira after the request token has been authorize.</param>
        /// <param name="oAuthTokenSecret">The token secret provided by Jira when asking for a request token.</param>
        /// <param name="oAuthSignatureMethod">The signature method used to generate the request token.</param>
        /// <param name="settings">Settings to configure the rest client.</param>
        /// <returns>Jira object configured to use REST API.</returns>
        public static Jira CreateOAuthRestClient(
            string url,
            string consumerKey,
            string consumerSecret,
            string oAuthAccessToken,
            string oAuthTokenSecret,
            JiraOAuthSignatureMethod oAuthSignatureMethod = JiraOAuthSignatureMethod.RsaSha1,
            JiraRestClientSettings settings = null)
        {
            settings = settings ?? new JiraRestClientSettings();
            var restClient = new JiraOAuthRestClient(
                url,
                consumerKey,
                consumerSecret,
                oAuthAccessToken,
                oAuthTokenSecret,
                oAuthSignatureMethod,
                settings);

            return CreateRestClient(restClient, settings.Cache);
        }

        /// <summary>
        /// Creates a JIRA client with the given rest client implementation.
        /// </summary>
        /// <param name="restClient">Rest client to use.</param>
        /// <param name="cache">Cache to use.</param>
        public static Jira CreateRestClient(IJiraRestClient restClient, JiraCache cache = null)
        {
            var services = new ServiceLocator();
            var jira = new Jira(services, cache);
            ConfigureDefaultServices(services, jira, restClient);
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
        /// Gets an object to interact with the issue remote links of jira.
        /// </summary>
        public IIssueRemoteLinkService RemoteLinks
        {
            get
            {
                return Services.Get<IIssueRemoteLinkService>();
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

        /// Gets an object to interact with the Jira screens.
        /// </summary>
        public IScreenService Screens
        {
            get
            {
                return Services.Get<IScreenService>();
            }
        }

        /// <summary>
        /// Gets an object to interact with the server information.
        /// </summary>
        public IServerInfoService ServerInfo
        {
            get
            {
                return Services.Get<IServerInfoService>();
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
        [Obsolete("Use Jira.Issues.MaxIssuesPerRequest")]
        public int MaxIssuesPerRequest
        {
            get
            {
                return this.Issues.MaxIssuesPerRequest;
            }
            set
            {
                this.Issues.MaxIssuesPerRequest = value;
            }
        }

        /// <summary>
        /// Url to the JIRA server
        /// </summary>
        public string Url
        {
            get { return RestClient.Url; }
        }

        internal IFileSystem FileSystem
        {
            get
            {
                return Services.Get<IFileSystem>();
            }
        }

        /// <summary>
        /// Returns a new issue that when saved will be created on the remote JIRA server.
        /// </summary>
        public Issue CreateIssue(string project, string parentIssueKey = null)
        {
            return new Issue(this, project, parentIssueKey);
        }

        /// <summary>
        /// Returns a new issue that when saved will be created on the remote JIRA server.
        /// </summary>
        public Issue CreateIssue(CreateIssueFields fields)
        {
            return new Issue(this, fields);
        }

        internal static string FormatDateTimeString(DateTime value)
        {
            /* Using "en-us" culture to conform to formats of JIRA.
             * See https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
             */
            return value.ToString(
                value.TimeOfDay == TimeSpan.Zero ? DEFAULT_DATE_FORMAT : DEFAULT_DATE_TIME_FORMAT,
                Jira.DefaultCultureInfo);
        }

        private static void ConfigureDefaultServices(ServiceLocator services, Jira jira, IJiraRestClient restClient)
        {
            services.Register<IProjectVersionService>(() => new ProjectVersionService(jira));
            services.Register<IProjectComponentService>(() => new ProjectComponentService(jira));
            services.Register<IIssuePriorityService>(() => new IssuePriorityService(jira));
            services.Register<IIssueResolutionService>(() => new IssueResolutionService(jira));
            services.Register<IIssueStatusService>(() => new IssueStatusService(jira));
            services.Register<IIssueLinkService>(() => new IssueLinkService(jira));
            services.Register<IIssueRemoteLinkService>(() => new IssueRemoteLinkService(jira));
            services.Register<IIssueTypeService>(() => new IssueTypeService(jira));
            services.Register<IIssueFilterService>(() => new IssueFilterService(jira));
            services.Register<IIssueFieldService>(() => new IssueFieldService(jira));
            services.Register<IIssueService>(() => new IssueService(jira, restClient.Settings));
            services.Register<IJiraUserService>(() => new JiraUserService(jira));
            services.Register<IJiraGroupService>(() => new JiraGroupService(jira));
            services.Register<IProjectService>(() => new ProjectService(jira));
            services.Register<IScreenService>(() => new ScreenService(jira));
            services.Register<IServerInfoService>(() => new ServerInfoService(jira));
            services.Register<IJqlExpressionVisitor>(() => new JqlExpressionVisitor());
            services.Register<IFileSystem>(() => new FileSystem());
            services.Register(() => restClient);
        }
    }
}

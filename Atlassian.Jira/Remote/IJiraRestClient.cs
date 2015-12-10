using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Contract for a client that iteracts with JIRA via rest.
    /// </summary>
    public interface IJiraRestClient
    {
        /// <summary>
        /// Executes a request.
        /// </summary>
        /// <param name="request">Request object.</param>
        IRestResponse ExecuteRequest(IRestRequest request);

        /// <summary>
        /// Executes a request and returns the response as JSON.
        /// </summary>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        JToken ExecuteRequest(Method method, string resource, object requestBody = null);

        /// <summary>
        /// Executes an async request and returns the response as JSON.
        /// </summary>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody = null);

        /// <summary>
        /// Executes an async request and returns the response as JSON.
        /// </summary>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody, CancellationToken token);

        /// <summary>
        /// Executes a request and serializes the response to an object.
        /// </summary>
        /// <typeparam name="T">Type to serialize the reponse.</typeparam>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        T ExecuteRequest<T>(Method method, string resource, object requestBody = null);

        /// <summary>
        /// Executes an async request and serializes the response to an object.
        /// </summary>
        /// <typeparam name="T">Type to serialize the reponse.</typeparam>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody = null);

        /// <summary>
        /// Executes an async request and serializes the response to an object.
        /// </summary>
        /// <typeparam name="T">Type to serialize the reponse.</typeparam>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody, CancellationToken token);

        /// <summary>
        /// Retrieves an issue by its key.
        /// </summary>
        /// <param name="issueKey">The issue key to retrieve</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<Issue> GetIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Updates an issue and returns a new instance populated from server.
        /// </summary>
        /// <param name="issue">Issue to update.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<Issue> UpdateIssueAsync(Issue issue, CancellationToken token);

        /// <summary>
        /// Creates an issue and returns a new instance populated from server.
        /// </summary>
        /// <param name="issue">Issue to create.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<Issue> CreateIssueAsyc(Issue issue, CancellationToken token);

        /// <summary>
        /// Transition an issue through a workflow action.
        /// </summary>
        /// <param name="issue">Issue to transition.</param>
        /// <param name="actionId">The workflow action to transition to.</param>
        /// <param name="updates">Additional updates to perform when transitioning the issue.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<Issue> ExecuteIssueWorkflowActionAsync(Issue issue, string actionId, WorkflowTransitionUpdates updates, CancellationToken token);

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <returns>Collection of Issues that match the search query</returns>
        Task<IEnumerable<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues = null, int startAt = 0);

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues, int startAt, CancellationToken token);

        /// <summary>
        /// Gets time tracking information for an issue.
        /// </summary>
        /// <param name="issueKey">The issue key.</param>
        IssueTimeTrackingData GetTimeTrackingData(string issueKey);

        /// <summary>
        /// Returns all custom fields within JIRA.
        /// </summary>
        Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken token);

        /// <summary>
        /// Returns the favourite filters for the user.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync(CancellationToken token);

        /// <summary>
        /// Returns all the issue priorities within JIRA.
        /// </summary>
        Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync(CancellationToken token);

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync(CancellationToken token);

        /// <summary>
        /// Returns all the issue statuses within JIRA.
        /// </summary>
        Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(CancellationToken token);

        /// <summary>
        /// Returns all the issue types within JIRA.
        /// </summary>
        Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token);

        /// <summary>
        /// Adds a comment to an issue.
        /// </summary>
        /// <param name="issueKey">Issue key to add the comment to.</param>
        /// <param name="comment">Comment object to add.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<Comment> AddCommentToIssueAsync(string issueKey, Comment comment, CancellationToken token);

        /// <summary>
        /// Returns the comments of an issue.
        /// </summary>
        /// <param name="issueKey">Issue key to retrieve comments from.</param>
        /// <param name="maxComments">Maximum number of comments to retrieve.</param>
        /// <param name="startAt">Index of the first comment to return (0-based).</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IPagedQueryResult<Comment>> GetCommentsFromIssueAsync(string issueKey, int maxComments, int startAt, CancellationToken token);

        /// <summary>
        /// Returns the workflow actions that an issue can be transitioned to.
        /// </summary>
        /// <param name="issueKey">The issue key</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraNamedEntity>> GetActionsForIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Returns all projects defined in JIRA.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken token);
    }
}

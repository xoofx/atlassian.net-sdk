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
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        Task<RemoteIssue[]> GetIssuesFromJqlSearchAsync(string jqlSearch, int maxResults, int startAt = 0);

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<RemoteIssue[]> GetIssuesFromJqlSearchAsync(string jqlSearch, int maxResults, int startAt, CancellationToken token);

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
    }
}

using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Contract for a client that iteracts with Jira via rest.
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
        /// Gets time tracking information for an issue.
        /// </summary>
        /// <param name="issueKey">The issue key.</param>
        IssueTimeTrackingData GetTimeTrackingData(string issueKey);

        /// <summary>
        /// Returns all custom fields within JIRA.
        /// </summary>
        Task<IEnumerable<CustomField>> GetCustomFieldsAsync();

        /// <summary>
        /// Returns the favourite filters for the user.
        /// </summary>
        Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync();

        /// <summary>
        /// Returns the favourite filters for the user.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync(CancellationToken token);

        /// <summary>
        /// Returns all the issue priorities within JIRA.
        /// </summary>
        Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync();

        /// <summary>
        /// Returns all the issue resolutions within JIRA
        /// </summary>
        Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync();

        /// <summary>
        /// Returns all the issue statuses within JIRA.
        /// </summary>
        Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync();

        /// <summary>
        /// Returns all the issue types within JIRA.
        /// </summary>
        Task<IEnumerable<IssueType>> GetIssueTypesAsync();
    }
}

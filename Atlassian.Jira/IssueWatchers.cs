using Atlassian.Jira.Remote;
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
    /// Represents the list of users that are watching an issue.
    /// </summary>
    public class IssueWatchers
    {
        private readonly IJiraRestClient _restClient;
        private readonly string _issueKey;
        private readonly string _resourceUrl;

        internal IssueWatchers(IJiraRestClient restClient, string issueKey)
        {
            _restClient = restClient;
            _issueKey = issueKey;
            _resourceUrl = String.Format("rest/api/2/issue/{0}/watchers", issueKey);
        }

        /// <summary>
        /// Removes a user from the watchers of the issue.
        /// </summary>
        /// <param name="username">Username of the user to add.</param>
        public void Remove(string username)
        {
            ExecuteAndGuard(() => RemoveAsync(username, CancellationToken.None).Wait());
        }

        /// <summary>
        /// Removes a user from the watchers of the issue.
        /// </summary>
        /// <param name="username">Username of the user to add.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task RemoveAsync(string username, CancellationToken token)
        {
            EnsureIssueCreated();

            var resourceUrl = String.Format("{0}?username={1}", _resourceUrl, System.Uri.EscapeDataString(username));
            return _restClient.ExecuteRequestAsync(Method.DELETE, resourceUrl, null, token);
        }

        /// <summary>
        /// Adds a user to the watchers of the issue.
        /// </summary>
        /// <param name="username"></param>
        public void Add(string username)
        {
            ExecuteAndGuard(() => AddAsync(username, CancellationToken.None).Wait());
        }

        /// <summary>
        /// Adds a user to the watchers of the issue.
        /// </summary>
        /// <param name="username">Username of the user to add.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task AddAsync(string username, CancellationToken token)
        {
            EnsureIssueCreated();

            var requestBody = String.Format("\"{0}\"", username);

            return _restClient.ExecuteRequestAsync(Method.POST, _resourceUrl, requestBody, token);
        }

        /// <summary>
        /// Gets the users that are watching the issue.
        /// </summary>
        public IEnumerable<JiraUser> Get()
        {
            try
            {
                return GetAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Gets the users that are watching the issue.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<JiraUser>> GetAsync(CancellationToken token)
        {
            EnsureIssueCreated();

            return _restClient.GetWatchersFromIssueAsync(this._issueKey, token);
        }

        private void EnsureIssueCreated()
        {
            if (string.IsNullOrEmpty(this._issueKey))
            {
                throw new InvalidOperationException("Unable to interact with the watchers resource, issue has not been created yet.");
            }
        }

        private void ExecuteAndGuard(Action execute)
        {
            try
            {
                execute();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }
    }
}

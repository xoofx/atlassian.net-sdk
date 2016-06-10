using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the list of labels that can be set on an issue.
    /// </summary>
    public class IssueLabels
    {
        private readonly IIssueService _restClient;
        private readonly RemoteIssue _remoteIssue;

        internal IssueLabels(IIssueService restClient, RemoteIssue remoteIssue)
        {
            _restClient = restClient;
            _remoteIssue = remoteIssue;
        }

        /// <summary>
        /// The list of labels when the issue was retrieved from the server.
        /// </summary>
        public string[] Cached
        {
            get
            {
                EnsureIssueCreated();
                return _remoteIssue.labelsReadOnly ?? new string[0];
            }
        }

        /// <summary>
        /// Sets the labels of the issue.
        /// </summary>
        /// <param name="labels">The label(s) to set.</param>
        public void Set(params string[] labels)
        {
            try
            {
                SetAsync(labels, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Sets the labels of the issue.
        /// </summary>
        /// <param name="labels">The label(s) to set.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task SetAsync(string[] labels, CancellationToken token)
        {
            EnsureIssueCreated();
            return _restClient.SetLabelsAsync(_remoteIssue.key, labels, token);
        }

        /// <summary>
        /// Gets the latest labels of the issue from the server.
        /// </summary>
        public string[] Get()
        {
            try
            {
                return this.GetAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Gets the latest labels of the issue from the server.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<string[]> GetAsync(CancellationToken token)
        {
            EnsureIssueCreated();

            return _restClient.GetLabelsAsync(_remoteIssue.key, token);
        }

        private void EnsureIssueCreated()
        {
            if (_remoteIssue == null || string.IsNullOrEmpty(_remoteIssue.key))
            {
                throw new InvalidOperationException("Unable to interact with the labels collection, issue has not been created yet.");
            }
        }
    }
}

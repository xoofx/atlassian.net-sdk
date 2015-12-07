using Newtonsoft.Json;
﻿using System;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
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
        /// Gets the global serializer settings to use.
        /// </summary>
        JsonSerializerSettings GetSerializerSettings();

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
        Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues = null, int startAt = 0);

        /// <summary>
        /// Execute a specific JQL query and return the resulting issues.
        /// </summary>
        /// <param name="jql">JQL search query</param>
        /// <param name="maxIssues">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
        /// <param name="startAt">Index of the first issue to return (0-based)</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues, int startAt, CancellationToken token);

        /// <summary>
        /// Gets time tracking information for an issue.
        /// </summary>
        /// <param name="issueKey">The issue key.</param>
        IssueTimeTrackingData GetTimeTrackingData(string issueKey);

        /// <summary>
        /// Gets metadata object containing dictionary with issuefields identifiers as keys and their metadata as values 
        /// </summary>
        /// <param name="issueKey">The issue key.</param>
        IDictionary<String, IssueFieldEditMetadata> GetIssueFieldsEditMetadata(string issueKey);

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
        /// Returns all available issue link types.
        /// </summary>
        Task<IEnumerable<IssueLinkType>> GetIssueLinkTypesAsync(CancellationToken token);

        /// <summary>
        /// Creates an issue link between two issues.
        /// </summary>
        /// <param name="outwardIssueKey">Key of the outward issue.</param>
        /// <param name="inwardIssueKey">Key of the inward issue.</param>
        /// <param name="linkName">Name of the issue link.</param>
        /// <param name="comment">Comment to add to the outward issue.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task LinkIssuesAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string comment, CancellationToken token);

        /// <summary>
        /// Returns all issue links associated with a given issue.
        /// </summary>
        /// <param name="issue">The issue to retrieve links for.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<IssueLink>> GetIssueLinksAsync(Issue issue, CancellationToken token);

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

        /// <summary>
        /// Retrieve attachment metadata from server for this issue
        /// </summary>
        /// <param name="issueKey">The issue key to get attachments from.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<Attachment>> GetAttachmentsFromIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Retrieve the labels from server for the issue specified.
        /// </summary>
        /// <param name="issueKey">The issue key to get labels from.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<string[]> GetLabelsFromIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Sets the labels for the issue specified.
        /// </summary>
        /// <param name="issueKey">The issue key to set the labels.</param>
        /// <param name="labels">The list of labels to set on the issue.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task SetLabelsForIssueAsync(string issueKey, string[] labels, CancellationToken token);

        /// <summary>
        /// Retrieve the watchers from server for the issue specified.
        /// </summary>
        /// <param name="issueKey">The issue key to get watchers from.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<JiraUser>> GetWatchersFromIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Retrieve the change logs from server for the issue specified.
        /// </summary>
        /// <param name="issueKey">The issue key to get watchers from.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<IEnumerable<IssueChangeLog>> GetChangeLogsFromIssueAsync(string issueKey, CancellationToken token);

        /// <summary>
        /// Retrieves a version by its id.
        /// </summary>
        /// <param name="versionId">The version id to retrieve</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<RemoteVersion> GetVersionAsync(string versionId, CancellationToken token);

        /// <summary>
        /// Updates a version and returns a new instance populated from server.
        /// </summary>
        /// <param name="version">Version to update.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        Task<RemoteVersion> UpdateVersionAsync(RemoteVersion version, CancellationToken token);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Abstract the proxy SOAP client implementation
    /// </summary>
    public interface IJiraSoapServiceClient
    {
        string Url { get; }
        string Login(string username, string password);
        RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults);
        RemoteIssue CreateIssue(string token, RemoteIssue newIssue);
        RemoteIssue CreateIssueWithParent(string token, RemoteIssue newIssue, string parentIssueKey);
        RemoteIssue UpdateIssue(string token, string key, RemoteFieldValue[] fields);
        RemoteAttachment[] GetAttachmentsFromIssue(string token, string key);
        bool AddBase64EncodedAttachmentsToIssue(string token, string key, string[] fileNames, string[] base64EncodedAttachmentData);
        RemoteComment[] GetCommentsFromIssue(string token, string key);
        void AddComment(string token, string key, RemoteComment comment);
        RemoteIssueType[] GetIssueTypes(string token, string projectId);
        RemotePriority[] GetPriorities(string token);
        RemoteResolution[] GetResolutions(string token);
        RemoteStatus[] GetStatuses(string token);
        RemoteVersion[] GetVersions(string token, string projectKey);
        RemoteComponent[] GetComponents(string token, string projectKey);
        RemoteField[] GetCustomFields(string token);
        RemoteField[] GetFieldsForEdit(string token, string key);
        RemoteWorklog AddWorklogAndAutoAdjustRemainingEstimate(string token, string key, RemoteWorklog worklog);
        RemoteWorklog AddWorklogAndRetainRemainingEstimate(string token, string key, RemoteWorklog worklog);
        RemoteWorklog AddWorklogWithNewRemainingEstimate(string token, string key, RemoteWorklog worklog, string newRemainingEstimate);
        RemoteWorklog[] GetWorkLogs(string token, string key);
        RemoteProject[] GetProjects(string token);
        RemoteFilter[] GetFavouriteFilters(string token);
        RemoteIssue[] GetIssuesFromFilterWithLimit(string token, string filterId, int offset, int maxResults);
        RemoteNamedObject[] GetAvailableActions(string token, string issueKey);
        RemoteIssue ProgressWorkflowAction(string token, string issueKey, string actionId, RemoteFieldValue[] remoteFieldValues);
    }
}

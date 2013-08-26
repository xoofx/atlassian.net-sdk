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
        void DeleteWorklogAndAutoAdjustRemainingEstimate(string token, string in1);
        void DeleteWorklogAndRetainRemainingEstimate(string token, string worklogId);
        void DeleteWorklogWithNewRemainingEstimate(string token, string worklogId, string newRemainingEstimate);
        void DeleteIssue(string token, string issueKey);

        /* Full list with un-edited arg names */
        void AddActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4);
        void AddDefaultActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3);
        RemotePermissionScheme AddPermissionTo(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3);
        void AddUserToGroup(string in0, RemoteGroup in1, RemoteUser in2);
        RemoteVersion AddVersion(string in0, string in1, RemoteVersion in2);
        void ArchiveVersion(string in0, string in1, string in2, bool in3);
        RemoteGroup CreateGroup(string in0, string in1, RemoteUser in2);
        RemoteIssue CreateIssueWithSecurityLevel(string in0, RemoteIssue in1, long in2);
        RemotePermissionScheme CreatePermissionScheme(string in0, string in1, string in2);
        RemoteProject CreateProject(string in0, string in1, string in2, string in3, string in4, string in5, RemotePermissionScheme in6, RemoteScheme in7, RemoteScheme in8);
        RemoteProject CreateProjectFromObject(string in0, RemoteProject in1);
        RemoteProjectRole CreateProjectRole(string in0, RemoteProjectRole in1);
        RemoteUser CreateUser(string in0, string in1, string in2, string in3, string in4);
        void DeleteGroup(string in0, string in1, string in2);
        RemotePermissionScheme DeletePermissionFrom(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3);
        void DeletePermissionScheme(string in0, string in1);
        void DeleteProject(string in0, string in1);
        void DeleteProjectAvatar(string in0, long in1);
        void DeleteProjectRole(string in0, RemoteProjectRole in1, bool in2);
        void DeleteUser(string in0, string in1);
        RemoteComment EditComment(string in0, RemoteComment in1);
        RemotePermission[] GetAllPermissions(string in0);
        RemoteScheme[] GetAssociatedNotificationSchemes(string in0, RemoteProjectRole in1);
        RemoteScheme[] GetAssociatedPermissionSchemes(string in0, RemoteProjectRole in1);
        RemoteComment GetComment(string in0, long in1);
        RemoteComment[] GetComments(string in0, string in1);
        RemoteConfiguration GetConfiguration(string in0);
        RemoteRoleActors GetDefaultRoleActors(string in0, RemoteProjectRole in1);
        RemoteField[] GetFieldsForAction(string in0, string in1, string in2);
        RemoteGroup GetGroup(string in0, string in1);
        RemoteIssue GetIssue(string in0, string in1);
        RemoteIssue GetIssueById(string in0, string in1);
        long GetIssueCountForFilter(string in0, string in1);
        RemoteIssue[] GetIssuesFromTextSearchWithLimit(string in0, string in1, int in2, int in3);
        RemoteIssue[] GetIssuesFromTextSearchWithProject(string in0, string[] in1, string in2, int in3);
        RemoteIssueType[] GetIssueTypes(string in0);
        RemoteIssueType[] GetIssueTypesForProject(string in0, string in1);
        RemoteScheme[] GetNotificationSchemes(string in0);
        RemotePermissionScheme[] GetPermissionSchemes(string in0);
        RemoteAvatar GetProjectAvatar(string in0, string in1);
        RemoteAvatar[] GetProjectAvatars(string in0, string in1, bool in2);
        RemoteProject GetProjectById(string in0, long in1);
        RemoteProject GetProjectByKey(string in0, string in1);
        RemoteProjectRole GetProjectRole(string in0, long in1);
        RemoteProjectRoleActors GetProjectRoleActors(string in0, RemoteProjectRole in1, RemoteProject in2);
        RemoteProjectRole[] GetProjectRoles(string in0);
        RemoteProject[] GetProjectsNoSchemes(string in0);
        RemoteProject GetProjectWithSchemesById(string in0, long in1);
        DateTime GetResolutionDateById(string in0, long in1);
        DateTime GetResolutionDateByKey(string in0, string in1);
        RemoteSecurityLevel GetSecurityLevel(string in0, string in1);
        RemoteSecurityLevel[] GetSecurityLevels(string in0, string in1);
        RemoteScheme[] GetSecuritySchemes(string in0);
        RemoteServerInfo GetServerInfo(string in0);
        RemoteIssueType[] GetSubTaskIssueTypes(string in0);
        RemoteIssueType[] GetSubTaskIssueTypesForProject(string in0, string in1);
        RemoteUser GetUser(string in0, string in1);
        RemoteWorklog[] GetWorklogs(string in0, string in1);
        bool HasPermissionToCreateWorklog(string in0, string in1);
        bool HasPermissionToDeleteWorklog(string in0, string in1);
        bool HasPermissionToEditComment(string in0, RemoteComment in1);
        bool HasPermissionToUpdateWorklog(string in0, string in1);
        bool IsProjectRoleNameUnique(string in0, string in1);
        bool Logout(string in0);
        void RefreshCustomFields(string in0);
        void ReleaseVersion(string in0, string in1, RemoteVersion in2);
        void RemoveActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4);
        void RemoveAllRoleActorsByNameAndType(string in0, string in1, string in2);
        void RemoveAllRoleActorsByProject(string in0, RemoteProject in1);
        void RemoveDefaultActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3);
        void RemoveUserFromGroup(string in0, RemoteGroup in1, RemoteUser in2);
        void SetNewProjectAvatar(string in0, string in1, string in2, string in3);
        void SetProjectAvatar(string in0, string in1, long in2);
        RemoteGroup UpdateGroup(string in0, RemoteGroup in1);
        RemoteProject UpdateProject(string in0, RemoteProject in1);
        void UpdateProjectRole(string in0, RemoteProjectRole in1);
        void UpdateWorklogAndAutoAdjustRemainingEstimate(string in0, RemoteWorklog in1);
        void UpdateWorklogAndRetainRemainingEstimate(string in0, RemoteWorklog in1);
        void UpdateWorklogWithNewRemainingEstimate(string in0, RemoteWorklog in1, string in2);
    }
}

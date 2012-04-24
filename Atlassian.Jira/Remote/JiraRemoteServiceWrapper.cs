using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Wraps the auto-generated JiraSoapServiceClient proxy
    /// </summary>
    internal class JiraRemoteServiceWrapper: IJiraRemoteService
    {
        private readonly JiraSoapServiceClient _client;
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;

        public JiraRemoteServiceWrapper(string jiraBaseUrl)
            : this(jiraBaseUrl, null, null)
        {
        }

        public JiraRemoteServiceWrapper(string jiraBaseUrl, string username, string password)
        {
            _client = JiraRemoteServiceFactory.Create(jiraBaseUrl);
            _url = jiraBaseUrl.EndsWith("/") ? jiraBaseUrl : jiraBaseUrl += "/";
            _username = username;
            _password = password;
        }

        public string Url
        {
            get
            {
                return _url;
            }
        }

        public string GetJsonFromJqlSearch(string jql, int startAt, int maxResults, string[] fields = null)
        {
            var restClient = new RestClient(_url);
            if (!String.IsNullOrEmpty(_username) && !String.IsNullOrEmpty(_password))
            {
                restClient.Authenticator = new HttpBasicAuthenticator(_username, _password);
            }

            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = "rest/api/latest/search";
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { jql = jql, startAt = startAt, maxResults = maxResults, fields = fields });

            var response = restClient.Execute(request);

            return response.Content;
        }

        public int GetIssueCountFromJqlSearch(string jql)
        {
            var json = JObject.Parse(GetJsonFromJqlSearch(jql, 0, 1, new string[1] { "summary" }));
            return (int) json["total"];
        }

        public string Login(string username, string password)
        {
            return _client.login(username, password);
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults)
        {
            return _client.getIssuesFromJqlSearch(token, jqlSearch, maxNumResults);
        }

        public RemoteIssue CreateIssue(string token, RemoteIssue newIssue)
        {
            
            return _client.createIssue(token, newIssue);
        }

        public RemoteIssue UpdateIssue(string token, string key, RemoteFieldValue[] fields)
        {
            return _client.updateIssue(token, key, fields);
        }

        public RemoteAttachment[] GetAttachmentsFromIssue(string token, string key)
        {
            return _client.getAttachmentsFromIssue(token, key);
        }

        public bool AddBase64EncodedAttachmentsToIssue(string token, string key, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            return _client.addBase64EncodedAttachmentsToIssue(token, key, fileNames, base64EncodedAttachmentData);
        }

        public RemoteComment[] GetCommentsFromIssue(string token, string key)
        {
            return _client.getComments(token, key);
        }

        public void AddComment(string token, string key, RemoteComment comment)
        {
            _client.addComment(token, key, comment);
        }

        public RemoteIssueType[] GetIssueTypes(string token, string projectId)
        {
            if (String.IsNullOrEmpty(projectId))
            {
                return _client.getIssueTypes(token);
            }
            else
            {
                return _client.getIssueTypesForProject(token, projectId);
            }
        }

        public RemotePriority[] GetPriorities(string token)
        {
            return _client.getPriorities(token);
        }

        public RemoteResolution[] GetResolutions(string token)
        {
            return _client.getResolutions(token);
        }

        public RemoteStatus[] GetStatuses(string token)
        {
            return _client.getStatuses(token);
        }

        public RemoteVersion[] GetVersions(string token, string projectKey)
        {
            return _client.getVersions(token, projectKey);
        }

        public RemoteComponent[] GetComponents(string token, string projectKey)
        {
            return _client.getComponents(token, projectKey);
        }

        public RemoteField[] GetCustomFields(string token)
        {
            return _client.getCustomFields(token);
        }

        public RemoteField[] GetFieldsForEdit(string token, string key)
        {
            return _client.getFieldsForEdit(token, key);
        }

        public RemoteWorklog AddWorklogAndAutoAdjustRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return _client.addWorklogAndAutoAdjustRemainingEstimate(token, key, worklog);
        }

        public RemoteWorklog AddWorklogAndRetainRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return _client.addWorklogAndRetainRemainingEstimate(token, key, worklog);
        }

        public RemoteWorklog AddWorklogWithNewRemainingEstimate(string token, string key, RemoteWorklog worklog, string newRemainingEstimate)
        {
            return _client.addWorklogWithNewRemainingEstimate(token, key, worklog, newRemainingEstimate);
        }

        public RemoteWorklog[] GetWorkLogs(string token, string key)
        {
            return _client.getWorklogs(token, key);
        }

        public RemoteProject[] GetProjects(string token)
        {
            return _client.getProjectsNoSchemes(token);
        }

        public RemoteIssue CreateIssueWithParent(string token, RemoteIssue newIssue, string parentIssueKey)
        {
            return _client.createIssueWithParent(token, newIssue, parentIssueKey);
        }

        public RemoteFilter[] GetFavouriteFilters(string token)
        {
            return _client.getFavouriteFilters(token);
        }

        public RemoteIssue[] GetIssuesFromFilterWithLimit(string token, string filterId, int offset, int maxResults)
        {
            return _client.getIssuesFromFilterWithLimit(token, filterId, offset, maxResults);
        }

        public RemoteNamedObject[] GetAvailableActions(string token, string issueKey)
        {
            return _client.getAvailableActions(token, issueKey);
        }

        public RemoteIssue ProgressWorkflowAction(string token, string issueKey, string actionId, RemoteFieldValue[] remoteFieldValues)
        {
            return _client.progressWorkflowAction(token, issueKey, actionId, remoteFieldValues);
        }


        public void AddActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4)
        {
            _client.addActorsToProjectRole(in0, in1, in2, in3, in4);
        }

        public void AddDefaultActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3)
        {
            _client.addDefaultActorsToProjectRole(in0, in1, in2, in3);
        }

        public RemotePermissionScheme AddPermissionTo(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3)
        {
            return _client.addPermissionTo(in0, in1, in2, in3);

        }

        public void AddUserToGroup(string in0, RemoteGroup in1, RemoteUser in2)
        {
            _client.addUserToGroup(in0, in1, in2);
        }

        public RemoteVersion AddVersion(string in0, string in1, RemoteVersion in2)
        {
            return _client.addVersion(in0, in1, in2);
        }

        public void ArchiveVersion(string in0, string in1, string in2, bool in3)
        {
            _client.archiveVersion(in0, in1, in2, in3);
        }

        public RemoteGroup CreateGroup(string in0, string in1, RemoteUser in2)
        {
            return _client.createGroup(in0, in1, in2);
        }

        public RemoteIssue CreateIssueWithSecurityLevel(string in0, RemoteIssue in1, long in2)
        {
            return _client.createIssueWithSecurityLevel(in0, in1, in2);
        }

        public RemotePermissionScheme CreatePermissionScheme(string in0, string in1, string in2)
        {
            return _client.createPermissionScheme(in0, in1, in2);
        }

        public RemoteProject CreateProject(string in0, string in1, string in2, string in3, string in4, string in5, RemotePermissionScheme in6, RemoteScheme in7, RemoteScheme in8)
        {
            return _client.createProject(in0, in1, in2, in3, in4, in5, in6, in7, in8);
        }

        public RemoteProject CreateProjectFromObject(string in0, RemoteProject in1)
        {
            return _client.createProjectFromObject(in0, in1);
        }

        public RemoteProjectRole CreateProjectRole(string in0, RemoteProjectRole in1)
        {
            return _client.createProjectRole(in0, in1);
        }

        public RemoteUser CreateUser(string in0, string in1, string in2, string in3, string in4)
        {
            return _client.createUser(in0, in1, in2, in3, in4);
        }

        public void DeleteGroup(string in0, string in1, string in2)
        {
            _client.deleteGroup(in0, in1, in2);
        }

        public void DeleteIssue(string in0, string in1)
        {
            _client.deleteIssue(in0, in1);
        }

        public RemotePermissionScheme DeletePermissionFrom(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3)
        {
            return _client.deletePermissionFrom(in0, in1, in2, in3);
        }

        public void DeletePermissionScheme(string in0, string in1)
        {
            _client.deletePermissionScheme(in0, in1);
        }

        public void DeleteProject(string in0, string in1)
        {
            _client.deleteProject(in0, in1);
        }

        public void DeleteProjectAvatar(string in0, long in1)
        {
            _client.deleteProjectAvatar(in0, in1);
        }

        public void DeleteProjectRole(string in0, RemoteProjectRole in1, bool in2)
        {
            _client.deleteProjectRole(in0, in1, in2);
        }

        public void DeleteUser(string in0, string in1)
        {
            _client.deleteUser(in0, in1);
        }

        public void DeleteWorklogAndAutoAdjustRemainingEstimate(string in0, string in1)
        {
            _client.deleteWorklogAndAutoAdjustRemainingEstimate(in0, in1);
        }

        public void DeleteWorklogAndRetainRemainingEstimate(string in0, string in1)
        {
            _client.deleteWorklogAndRetainRemainingEstimate(in0, in1);
        }

        public void DeleteWorklogWithNewRemainingEstimate(string in0, string in1, string in2)
        {
            _client.deleteWorklogWithNewRemainingEstimate(in0, in1, in2);
        }

        public RemoteComment EditComment(string in0, RemoteComment in1)
        {
            return _client.editComment(in0, in1);
        }

        public RemotePermission[] GetAllPermissions(string in0)
        {
            return _client.getAllPermissions(in0);
        }

        public RemoteScheme[] GetAssociatedNotificationSchemes(string in0, RemoteProjectRole in1)
        {
            return _client.getAssociatedNotificationSchemes(in0, in1);
        }

        public RemoteScheme[] GetAssociatedPermissionSchemes(string in0, RemoteProjectRole in1)
        {
            return _client.getAssociatedPermissionSchemes(in0, in1);
        }

        public RemoteComment GetComment(string in0, long in1)
        {
            return _client.getComment(in0, in1);
        }

        public RemoteComment[] GetComments(string in0, string in1)
        {
            return _client.getComments(in0, in1);
        }

        public RemoteConfiguration GetConfiguration(string in0)
        {
            return _client.getConfiguration(in0);
        }

        public RemoteRoleActors GetDefaultRoleActors(string in0, RemoteProjectRole in1)
        {
            return _client.getDefaultRoleActors(in0, in1);
        }

        public RemoteField[] GetFieldsForAction(string in0, string in1, string in2)
        {
            return _client.getFieldsForAction(in0, in1, in2);
        }

        public RemoteGroup GetGroup(string in0, string in1)
        {
            return _client.getGroup(in0, in1);
        }

        public RemoteIssue GetIssue(string in0, string in1)
        {
            return _client.getIssue(in0, in1);
        }

        public RemoteIssue GetIssueById(string in0, string in1)
        {
            return _client.getIssueById(in0, in1);
        }

        public long GetIssueCountForFilter(string in0, string in1)
        {
            return _client.getIssueCountForFilter(in0, in1);
        }

        public RemoteIssue[] GetIssuesFromTextSearchWithLimit(string in0, string in1, int in2, int in3)
        {
            return _client.getIssuesFromTextSearchWithLimit(in0, in1, in2, in3);
        }

        public RemoteIssue[] GetIssuesFromTextSearchWithProject(string in0, string[] in1, string in2, int in3)
        {
            return _client.getIssuesFromTextSearchWithProject(in0, in1, in2, in3);
        }

        public RemoteIssueType[] GetIssueTypes(string in0)
        {
            return _client.getIssueTypes(in0);
        }

        public RemoteIssueType[] GetIssueTypesForProject(string in0, string in1)
        {
            return _client.getIssueTypesForProject(in0, in1);
        }

        public RemoteScheme[] GetNotificationSchemes(string in0)
        {
            return _client.getNotificationSchemes(in0);
        }

        public RemotePermissionScheme[] GetPermissionSchemes(string in0)
        {
            return _client.getPermissionSchemes(in0);
        }

        public RemoteAvatar GetProjectAvatar(string in0, string in1)
        {
            return _client.getProjectAvatar(in0, in1);
        }

        public RemoteAvatar[] GetProjectAvatars(string in0, string in1, bool in2)
        {
            return _client.getProjectAvatars(in0, in1, in2);
        }

        public RemoteProject GetProjectById(string in0, long in1)
        {
            return _client.getProjectById(in0, in1);
        }

        public RemoteProject GetProjectByKey(string in0, string in1)
        {
            return _client.getProjectByKey(in0, in1);
        }

        public RemoteProjectRole GetProjectRole(string in0, long in1)
        {
            return _client.getProjectRole(in0, in1);
        }

        public RemoteProjectRoleActors GetProjectRoleActors(string in0, RemoteProjectRole in1, RemoteProject in2)
        {
            return _client.getProjectRoleActors(in0, in1, in2);
        }

        public RemoteProjectRole[] GetProjectRoles(string in0)
        {
            return _client.getProjectRoles(in0);
        }

        public RemoteProject[] GetProjectsNoSchemes(string in0)
        {
            return _client.getProjectsNoSchemes(in0);
        }

        public RemoteProject GetProjectWithSchemesById(string in0, long in1)
        {
            return _client.getProjectWithSchemesById(in0, in1);
        }

        public DateTime GetResolutionDateById(string in0, long in1)
        {
            return _client.getResolutionDateById(in0, in1);
        }

        public DateTime GetResolutionDateByKey(string in0, string in1)
        {
            return _client.getResolutionDateByKey(in0, in1);
        }

        public RemoteSecurityLevel GetSecurityLevel(string in0, string in1)
        {
            return _client.getSecurityLevel(in0, in1);
        }

        public RemoteSecurityLevel[] GetSecurityLevels(string in0, string in1)
        {
            return _client.getSecurityLevels(in0, in1);
        }

        public RemoteScheme[] GetSecuritySchemes(string in0)
        {
            return _client.getSecuritySchemes(in0);
        }

        public RemoteServerInfo GetServerInfo(string in0)
        {
            return _client.getServerInfo(in0);
        }

        public RemoteIssueType[] GetSubTaskIssueTypes(string in0)
        {
            return _client.getSubTaskIssueTypes(in0);
        }

        public RemoteIssueType[] GetSubTaskIssueTypesForProject(string in0, string in1)
        {
            return _client.getSubTaskIssueTypesForProject(in0, in1);
        }

        public RemoteUser GetUser(string in0, string in1)
        {
            return _client.getUser(in0, in1);
        }

        public RemoteWorklog[] GetWorklogs(string in0, string in1)
        {
            return _client.getWorklogs(in0, in1);
        }

        public bool HasPermissionToCreateWorklog(string in0, string in1)
        {
            return _client.hasPermissionToCreateWorklog(in0, in1);
        }

        public bool HasPermissionToDeleteWorklog(string in0, string in1)
        {
            return _client.hasPermissionToDeleteWorklog(in0, in1);
        }

        public bool HasPermissionToEditComment(string in0, RemoteComment in1)
        {
            return _client.hasPermissionToEditComment(in0, in1);
        }

        public bool HasPermissionToUpdateWorklog(string in0, string in1)
        {
            return _client.hasPermissionToUpdateWorklog(in0, in1);
        }

        public bool IsProjectRoleNameUnique(string in0, string in1)
        {
            return _client.isProjectRoleNameUnique(in0, in1);
        }

        public bool Logout(string in0)
        {
            return _client.logout(in0);
        }

        public void RefreshCustomFields(string in0)
        {
            _client.refreshCustomFields(in0);
        }

        public void ReleaseVersion(string in0, string in1, RemoteVersion in2)
        {
            _client.releaseVersion(in0, in1, in2);
        }

        public void RemoveActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4)
        {
            _client.removeActorsFromProjectRole(in0, in1, in2, in3, in4);
        }

        public void RemoveAllRoleActorsByNameAndType(string in0, string in1, string in2)
        {
            _client.removeAllRoleActorsByNameAndType(in0, in1, in2);
        }

        public void RemoveAllRoleActorsByProject(string in0, RemoteProject in1)
        {
            _client.removeAllRoleActorsByProject(in0, in1);
        }

        public void RemoveDefaultActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3)
        {
            _client.removeDefaultActorsFromProjectRole(in0, in1, in2, in3);
        }

        public void RemoveUserFromGroup(string in0, RemoteGroup in1, RemoteUser in2)
        {
            _client.removeUserFromGroup(in0, in1, in2);
        }

        public void SetNewProjectAvatar(string in0, string in1, string in2, string in3)
        {
            _client.setNewProjectAvatar(in0, in1, in2, in3);
        }

        public void SetProjectAvatar(string in0, string in1, long in2)
        {
            _client.setProjectAvatar(in0, in1, in2);
        }

        public RemoteGroup UpdateGroup(string in0, RemoteGroup in1)
        {
            return _client.updateGroup(in0, in1);
        }

        public RemoteProject UpdateProject(string in0, RemoteProject in1)
        {
            return _client.updateProject(in0, in1);
        }

        public void UpdateProjectRole(string in0, RemoteProjectRole in1)
        {
            _client.updateProjectRole(in0, in1);
        }

        public void UpdateWorklogAndAutoAdjustRemainingEstimate(string in0, RemoteWorklog in1)
        {
            _client.updateWorklogAndAutoAdjustRemainingEstimate(in0, in1);
        }

        public void UpdateWorklogAndRetainRemainingEstimate(string in0, RemoteWorklog in1)
        {
            _client.updateWorklogAndRetainRemainingEstimate(in0, in1);
        }

        public void UpdateWorklogWithNewRemainingEstimate(string in0, RemoteWorklog in1, string in2)
        {
            _client.updateWorklogWithNewRemainingEstimate(in0, in1, in2);
        }


        
    }
}
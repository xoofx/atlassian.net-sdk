using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Implements the service client contract using REST calls.
    /// </summary>
    internal class JiraRestServiceClient : IJiraServiceClient
    {
        private readonly RestClient _restClient;
        private readonly string _url;
        private readonly bool _enableTrace;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly RemoteField[] _customFields;
        private readonly Dictionary<string, ICustomFieldValueSerializer> _customFieldSerializers;

        public JiraRestServiceClient(string jiraBaseUrl, string username, string password, JiraRestClientSettings settings)
        {
            this._customFieldSerializers = new Dictionary<string, ICustomFieldValueSerializer>(settings.CustomFieldSerializers, StringComparer.InvariantCultureIgnoreCase);
            this._enableTrace = settings.EnableTrace;
            this._url = jiraBaseUrl.EndsWith("/") ? jiraBaseUrl : jiraBaseUrl += "/";
            this._restClient = new RestClient(this._url);

            this._serializerSettings = new JsonSerializerSettings();
            this._serializerSettings.NullValueHandling = NullValueHandling.Ignore;

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                this._restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            }

            // retrieve the custom fields once.
            this._customFields = this.GetCustomFieldsInternal(null).Where(f => f.IsCustomField).ToArray();
            this._serializerSettings.Converters.Add(new RemoteIssueJsonConverter(this._customFields, this._customFieldSerializers));
        }

        public string Url
        {
            get
            {
                return this._url;
            }
        }

        public string Login(string username, string password)
        {
            // Rest api does not have a login method and does not make use of access tokens.
            return "<Unused>";
        }

        private void LogRequest(RestRequest request, object body = null)
        {
            if (this._enableTrace)
            {
                Trace.WriteLine(String.Format("[{0}] Request Url: {1}",
                    request.Method,
                    request.Resource));

                if (body != null)
                {
                    Trace.WriteLine(String.Format("[{0}] Request Data: {1}",
                        request.Method,
                        JsonConvert.SerializeObject(body, new JsonSerializerSettings()
                        {
                            Formatting = Formatting.Indented,
                            NullValueHandling = NullValueHandling.Ignore
                        })));
                }
            }
        }

        private void EnsureValidResponse(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException(response.Content);
            }
        }

        private JToken ExecuteRequestWithData(string resource, object requestBody, Method method = Method.POST)
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new RestSharpJsonSerializer(JsonSerializer.Create(this._serializerSettings));
            request.AddJsonBody(requestBody);

            LogRequest(request, requestBody);
            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);

            return response.StatusCode != HttpStatusCode.NoContent ? JToken.Parse(response.Content) : new JObject();
        }

        private JToken ExecuteRequest(string resource, Method method = Method.GET)
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;

            LogRequest(request);
            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);

            return response.StatusCode != HttpStatusCode.NoContent ? JToken.Parse(response.Content) : new JObject();
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxResults, int startAt)
        {
            var responseObj = ExecuteRequestWithData("rest/api/2/search", new
            {
                jql = jqlSearch,
                startAt = startAt,
                maxResults = maxResults,
            });

            var issues = (JArray)responseObj["issues"];
            return issues.Cast<JObject>().Select(issueJson => JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), this._serializerSettings).RemoteIssue).ToArray();
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults)
        {
            return this.GetIssuesFromJqlSearch(token, jqlSearch, maxNumResults, 0);
        }

        public RemoteIssue CreateIssue(string token, RemoteIssue newIssue)
        {
            return CreateIssueWithParent(token, newIssue, null);
        }

        public RemoteIssue CreateIssueWithParent(string token, RemoteIssue newIssue, string parentIssueKey)
        {
            var responseObj = ExecuteRequestWithData("rest/api/2/issue", new RemoteIssueWrapper(newIssue, parentIssueKey));
            return this.GetIssue(token, (string)responseObj["key"]);
        }

        public RemoteVersion[] GetVersions(string token, string projectKey)
        {
            var resource = String.Format("rest/api/2/project/{0}/versions", projectKey);
            var versions = ExecuteRequest(resource);
            return JsonConvert.DeserializeObject<RemoteVersion[]>(versions.ToString(), _serializerSettings);
        }

        public RemoteComponent[] GetComponents(string token, string projectKey)
        {
            var resource = String.Format("rest/api/2/project/{0}/components", projectKey);
            var components = ExecuteRequest(resource);
            return JsonConvert.DeserializeObject<RemoteComponent[]>(components.ToString(), _serializerSettings);
        }

        public RemotePriority[] GetPriorities(string token)
        {
            var priorities = ExecuteRequest("rest/api/2/priority");
            return JsonConvert.DeserializeObject<RemotePriority[]>(priorities.ToString(), _serializerSettings);
        }

        public RemoteField[] GetCustomFields(string token)
        {
            return this._customFields;
        }

        public RemoteField[] GetCustomFieldsInternal(string token)
        {
            var fields = ExecuteRequest("rest/api/2/field");
            return JsonConvert.DeserializeObject<RemoteField[]>(fields.ToString(), _serializerSettings)
                .Where(f => f.IsCustomField).ToArray();
        }

        public RemoteField[] GetFieldsForEdit(string token, string key)
        {
            // TODO: this needs to be a different call.
            return this.GetCustomFields(token);
        }

        public RemoteIssue GetIssue(string token, string issueId)
        {
            var resource = String.Format("rest/api/2/issue/{0}", issueId);
            var issueJson = ExecuteRequest(resource);

            return JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), this._serializerSettings).RemoteIssue;
        }

        public RemoteIssue GetIssueById(string token, string issueId)
        {
            return GetIssue(token, issueId);
        }

        public RemoteProject[] GetProjects(string token)
        {
            var projects = ExecuteRequest("rest/api/2/project");
            return JsonConvert.DeserializeObject<RemoteProject[]>(projects.ToString(), _serializerSettings);
        }

        public RemoteIssueType[] GetIssueTypes(string token, string projectId)
        {
            var issueTypes = ExecuteRequest("rest/api/2/issuetype");
            return JsonConvert.DeserializeObject<RemoteIssueType[]>(issueTypes.ToString(), _serializerSettings);
        }

        public DateTime GetResolutionDateByKey(string token, string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=resolutiondate", issueKey);
            var issueJson = ExecuteRequest(resource);
            var fields = issueJson["fields"];
            var resolutionDate = fields["resolutiondate"];

            return resolutionDate.Type == JTokenType.Null ? DateTime.MinValue : (DateTime)resolutionDate;
        }

        public RemoteFilter[] GetFavouriteFilters(string token)
        {
            var filters = ExecuteRequest("rest/api/2/filter/favourite");
            return JsonConvert.DeserializeObject<RemoteFilter[]>(filters.ToString(), _serializerSettings);
        }

        public RemoteIssue[] GetIssuesFromFilterWithLimit(string token, string filterId, int offset, int maxResults)
        {
            var resource = String.Format("rest/api/2/filter/{0}", filterId);
            var jql = ExecuteRequest(resource)["jql"].ToString();

            return this.GetIssuesFromJqlSearch(token, jql, maxResults, offset);
        }

        public RemoteStatus[] GetStatuses(string token)
        {
            var statuses = ExecuteRequest("rest/api/2/status");
            return JsonConvert.DeserializeObject<RemoteStatus[]>(statuses.ToString(), _serializerSettings);
        }

        public RemoteResolution[] GetResolutions(string token)
        {
            var resolutions = ExecuteRequest("rest/api/2/resolution");
            return JsonConvert.DeserializeObject<RemoteResolution[]>(resolutions.ToString(), _serializerSettings);
        }

        public RemoteComment[] GetCommentsFromIssue(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=comment", key);
            var issueJson = ExecuteRequest(resource);
            var comments = issueJson["fields"]["comment"]["comments"];

            return JsonConvert.DeserializeObject<RemoteComment[]>(comments.ToString(), _serializerSettings);
        }

        public void AddComment(string token, string key, RemoteComment comment)
        {
            var resource = String.Format("rest/api/2/issue/{0}/comment", key);
            var responseObj = ExecuteRequestWithData(resource, comment);
        }

        public RemoteAttachment[] GetAttachmentsFromIssue(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=attachment", key);
            var issueJson = ExecuteRequest(resource);
            var attachments = issueJson["fields"]["attachment"];

            return JsonConvert.DeserializeObject<RemoteAttachment[]>(attachments.ToString(), _serializerSettings);
        }

        public bool AddBase64EncodedAttachmentsToIssue(string token, string key, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            var resource = String.Format("rest/api/2/issue/{0}/attachments", key);
            var request = new RestRequest();
            request.Method = Method.POST;
            request.Resource = resource;
            request.AddHeader("X-Atlassian-Token", "nocheck");
            request.AlwaysMultipartFormData = true;

            for (var i = 0; i < fileNames.Length; i++)
            {
                request.AddFile("file", Convert.FromBase64String(base64EncodedAttachmentData[i]), fileNames[i]);
            }

            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);

            return true;
        }

        public RemoteNamedObject[] GetAvailableActions(string token, string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}/transitions", issueKey);
            var transitions = ExecuteRequest(resource)["transitions"];
            return JsonConvert.DeserializeObject<RemoteNamedObject[]>(transitions.ToString(), _serializerSettings);
        }

        public void AddLabels(string token, RemoteIssue remoteIssue, string[] labels)
        {
            var resource = String.Format("rest/api/2/issue/{0}", remoteIssue.key);
            ExecuteRequestWithData(resource, new
            {
                fields = new
                {
                    labels = labels
                }

            }, Method.PUT);
        }

        private JObject BuildFieldsObjectFromIssue(RemoteIssue remoteIssue, RemoteFieldValue[] remoteFields)
        {
            var issueWrapper = new RemoteIssueWrapper(remoteIssue);
            var issueJson = JsonConvert.SerializeObject(issueWrapper, this._serializerSettings);
            var issueFields = JObject.Parse(issueJson)["fields"] as JObject;
            var updateFields = new JObject();

            foreach (var field in remoteFields)
            {
                updateFields.Add(field.id, issueFields[field.id]);
            }

            return updateFields;
        }

        public RemoteIssue UpdateIssue(string token, RemoteIssue remoteIssue, RemoteFieldValue[] remoteFields)
        {
            var resource = String.Format("rest/api/2/issue/{0}", remoteIssue.key);
            var fields = BuildFieldsObjectFromIssue(remoteIssue, remoteFields);

            ExecuteRequestWithData(resource, new { fields = fields }, Method.PUT);

            return this.GetIssue(token, remoteIssue.key);
        }

        public RemoteIssue ProgressWorkflowAction(string token, RemoteIssue remoteIssue, string actionId, RemoteFieldValue[] remoteFields)
        {
            var resource = String.Format("rest/api/2/issue/{0}/transitions", remoteIssue.key);
            var fields = BuildFieldsObjectFromIssue(remoteIssue, remoteFields);
            ExecuteRequestWithData(resource, new
            {
                transition = new
                {
                    id = actionId
                },
                fields = fields
            });

            return this.GetIssue(token, remoteIssue.key);
        }

        public void DeleteIssue(string token, string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}", issueKey);
            var versions = ExecuteRequest(resource, Method.DELETE);
        }

        public RemoteIssueType[] GetSubTaskIssueTypes(string token)
        {
            var issueTypesJson = ExecuteRequest("rest/api/2/issuetype");
            var issueTypes = JsonConvert.DeserializeObject<RemoteIssueType[]>(issueTypesJson.ToString(), _serializerSettings);

            return issueTypes.Where(i => i.subTask).ToArray();
        }

        public RemoteWorklog[] GetWorkLogs(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}/worklog", key);
            var worklogs = ExecuteRequest(resource)["worklogs"];

            return JsonConvert.DeserializeObject<RemoteWorklog[]>(worklogs.ToString(), _serializerSettings);
        }

        public RemoteWorklog AddWorklog(string token, string key, RemoteWorklog worklog, string queryString = null)
        {
            var resource = String.Format("rest/api/2/issue/{0}/worklog?{1}", key, queryString);
            var response = ExecuteRequestWithData(resource, worklog);
            return this.GetWorkLogs(token, key).First(w => w.id == (string)response["id"]);
        }

        public RemoteWorklog AddWorklogAndAutoAdjustRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return AddWorklog(token, key, worklog);
        }

        public RemoteWorklog AddWorklogAndRetainRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return AddWorklog(token, key, worklog, "adjustEstimate=leave");
        }

        public RemoteWorklog AddWorklogWithNewRemainingEstimate(string token, string key, RemoteWorklog worklog, string newRemainingEstimate)
        {
            return AddWorklog(token, key, worklog, "adjustEstimate=new&newEstimate=" + newRemainingEstimate);
        }

        private void DeleteWorklog(string token, string issueKey, string worklogId, string queryString = null)
        {
            var resource = String.Format("rest/api/2/issue/{0}/worklog/{1}?{2}", issueKey, worklogId, queryString);
            ExecuteRequest(resource, Method.DELETE);
        }

        public void DeleteWorklogAndAutoAdjustRemainingEstimate(string token, string issueKey, string worklogId)
        {
            DeleteWorklog(token, issueKey, worklogId);
        }

        public void DeleteWorklogAndRetainRemainingEstimate(string token, string issueKey, string worklogId)
        {
            DeleteWorklog(token, issueKey, worklogId, "adjustEstimate=leave");
        }

        public void DeleteWorklogWithNewRemainingEstimate(string token, string issueKey, string worklogId, string newRemainingEstimate)
        {
            DeleteWorklog(token, issueKey, worklogId, "adjustEstimate=new&newEstimate=" + newRemainingEstimate);
        }

        public void AddActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4)
        {
            throw new NotImplementedException();
        }

        public void AddDefaultActorsToProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3)
        {
            throw new NotImplementedException();
        }

        public RemotePermissionScheme AddPermissionTo(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3)
        {
            throw new NotImplementedException();
        }

        public void AddUserToGroup(string in0, RemoteGroup in1, RemoteUser in2)
        {
            throw new NotImplementedException();
        }

        public RemoteVersion AddVersion(string in0, string in1, RemoteVersion in2)
        {
            throw new NotImplementedException();
        }

        public void ArchiveVersion(string in0, string in1, string in2, bool in3)
        {
            throw new NotImplementedException();
        }

        public RemoteGroup CreateGroup(string in0, string in1, RemoteUser in2)
        {
            throw new NotImplementedException();
        }

        public RemoteIssue CreateIssueWithSecurityLevel(string in0, RemoteIssue in1, long in2)
        {
            throw new NotImplementedException();
        }

        public RemotePermissionScheme CreatePermissionScheme(string in0, string in1, string in2)
        {
            throw new NotImplementedException();
        }

        public RemoteProject CreateProject(string in0, string in1, string in2, string in3, string in4, string in5, RemotePermissionScheme in6, RemoteScheme in7, RemoteScheme in8)
        {
            throw new NotImplementedException();
        }

        public RemoteProject CreateProjectFromObject(string in0, RemoteProject in1)
        {
            throw new NotImplementedException();
        }

        public RemoteProjectRole CreateProjectRole(string in0, RemoteProjectRole in1)
        {
            throw new NotImplementedException();
        }

        public RemoteUser CreateUser(string in0, string in1, string in2, string in3, string in4)
        {
            throw new NotImplementedException();
        }

        public void DeleteGroup(string in0, string in1, string in2)
        {
            throw new NotImplementedException();
        }

        public RemotePermissionScheme DeletePermissionFrom(string in0, RemotePermissionScheme in1, RemotePermission in2, RemoteEntity in3)
        {
            throw new NotImplementedException();
        }

        public void DeletePermissionScheme(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public void DeleteProject(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public void DeleteProjectAvatar(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public void DeleteProjectRole(string in0, RemoteProjectRole in1, bool in2)
        {
            throw new NotImplementedException();
        }

        public void DeleteUser(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteComment EditComment(string in0, RemoteComment in1)
        {
            throw new NotImplementedException();
        }

        public RemotePermission[] GetAllPermissions(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteScheme[] GetAssociatedNotificationSchemes(string in0, RemoteProjectRole in1)
        {
            throw new NotImplementedException();
        }

        public RemoteScheme[] GetAssociatedPermissionSchemes(string in0, RemoteProjectRole in1)
        {
            throw new NotImplementedException();
        }

        public RemoteComment GetComment(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public RemoteComment[] GetComments(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteConfiguration GetConfiguration(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteRoleActors GetDefaultRoleActors(string in0, RemoteProjectRole in1)
        {
            throw new NotImplementedException();
        }

        public RemoteField[] GetFieldsForAction(string in0, string in1, string in2)
        {
            throw new NotImplementedException();
        }

        public RemoteGroup GetGroup(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public long GetIssueCountForFilter(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteIssue[] GetIssuesFromTextSearchWithLimit(string in0, string in1, int in2, int in3)
        {
            throw new NotImplementedException();
        }

        public RemoteIssue[] GetIssuesFromTextSearchWithProject(string in0, string[] in1, string in2, int in3)
        {
            throw new NotImplementedException();
        }

        public RemoteIssueType[] GetIssueTypes(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteIssueType[] GetIssueTypesForProject(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteScheme[] GetNotificationSchemes(string in0)
        {
            throw new NotImplementedException();
        }

        public RemotePermissionScheme[] GetPermissionSchemes(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteAvatar GetProjectAvatar(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteAvatar[] GetProjectAvatars(string in0, string in1, bool in2)
        {
            throw new NotImplementedException();
        }

        public RemoteProject GetProjectById(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public RemoteProject GetProjectByKey(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteProjectRole GetProjectRole(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public RemoteProjectRoleActors GetProjectRoleActors(string in0, RemoteProjectRole in1, RemoteProject in2)
        {
            throw new NotImplementedException();
        }

        public RemoteProjectRole[] GetProjectRoles(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteProject[] GetProjectsNoSchemes(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteProject GetProjectWithSchemesById(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public DateTime GetResolutionDateById(string in0, long in1)
        {
            throw new NotImplementedException();
        }

        public RemoteSecurityLevel GetSecurityLevel(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteSecurityLevel[] GetSecurityLevels(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteScheme[] GetSecuritySchemes(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteServerInfo GetServerInfo(string in0)
        {
            throw new NotImplementedException();
        }

        public RemoteIssueType[] GetSubTaskIssueTypesForProject(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteUser GetUser(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public RemoteWorklog[] GetWorklogs(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public bool HasPermissionToCreateWorklog(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public bool HasPermissionToDeleteWorklog(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public bool HasPermissionToEditComment(string in0, RemoteComment in1)
        {
            throw new NotImplementedException();
        }

        public bool HasPermissionToUpdateWorklog(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public bool IsProjectRoleNameUnique(string in0, string in1)
        {
            throw new NotImplementedException();
        }

        public bool Logout(string in0)
        {
            throw new NotImplementedException();
        }

        public void RefreshCustomFields(string in0)
        {
            throw new NotImplementedException();
        }

        public void ReleaseVersion(string in0, string in1, RemoteVersion in2)
        {
            throw new NotImplementedException();
        }

        public void RemoveActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, RemoteProject in3, string in4)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllRoleActorsByNameAndType(string in0, string in1, string in2)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllRoleActorsByProject(string in0, RemoteProject in1)
        {
            throw new NotImplementedException();
        }

        public void RemoveDefaultActorsFromProjectRole(string in0, string[] in1, RemoteProjectRole in2, string in3)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserFromGroup(string in0, RemoteGroup in1, RemoteUser in2)
        {
            throw new NotImplementedException();
        }

        public void SetNewProjectAvatar(string in0, string in1, string in2, string in3)
        {
            throw new NotImplementedException();
        }

        public void SetProjectAvatar(string in0, string in1, long in2)
        {
            throw new NotImplementedException();
        }

        public RemoteGroup UpdateGroup(string in0, RemoteGroup in1)
        {
            throw new NotImplementedException();
        }

        public RemoteProject UpdateProject(string in0, RemoteProject in1)
        {
            throw new NotImplementedException();
        }

        public void UpdateProjectRole(string in0, RemoteProjectRole in1)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorklogAndAutoAdjustRemainingEstimate(string in0, RemoteWorklog in1)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorklogAndRetainRemainingEstimate(string in0, RemoteWorklog in1)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorklogWithNewRemainingEstimate(string in0, RemoteWorklog in1, string in2)
        {
            throw new NotImplementedException();
        }
    }
}

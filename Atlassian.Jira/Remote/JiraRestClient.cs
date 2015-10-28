using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Implements the service client contract using REST calls.
    /// </summary>
    internal class JiraRestClient : IJiraClient
    {
        private readonly RestClient _restClient;
        private readonly JiraRestClientSettings _clientSettings;

        private JsonSerializerSettings _serializerSettings;
        private RemoteField[] _customFields;

        public JiraRestClient(string jiraBaseUrl, string username = null, string password = null, JiraRestClientSettings settings = null)
        {
            var url = jiraBaseUrl.EndsWith("/") ? jiraBaseUrl : jiraBaseUrl += "/";

            this._clientSettings = settings ?? new JiraRestClientSettings();
            this._restClient = new RestClient(url);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                this._restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            }
        }

        private JsonSerializerSettings GetSerializerSettings()
        {
            if (this._serializerSettings == null)
            {
                var serializers = new Dictionary<string, ICustomFieldValueSerializer>(this._clientSettings.CustomFieldSerializers, StringComparer.InvariantCultureIgnoreCase);
                var customFields = this.GetCustomFields(null);

                this._serializerSettings = new JsonSerializerSettings();
                this._serializerSettings.NullValueHandling = NullValueHandling.Ignore;
                this._serializerSettings.Converters.Add(new RemoteIssueJsonConverter(customFields, serializers));
            }

            return this._serializerSettings;
        }

        #region IJiraRestClient
        public JToken ExecuteRequest(Method method, string resource, object requestBody = null)
        {
            try
            {
                return ExecuteRequestAsync(method, resource, requestBody).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        public T ExecuteRequest<T>(Method method, string resource, object requestBody = null)
        {
            try
            {
                return ExecuteRequestAsync<T>(method, resource, requestBody).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        public Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody = null)
        {
            return this.ExecuteRequestAsync<T>(method, resource, requestBody, CancellationToken.None);
        }

        public Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody, CancellationToken token)
        {
            return ExecuteRequestAsync(method, resource, requestBody, token).ContinueWith<T>(responseTask =>
            {
                return JsonConvert.DeserializeObject<T>(responseTask.Result.ToString(), _serializerSettings);
            });
        }

        public Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody = null)
        {
            return this.ExecuteRequestAsync(method, resource, requestBody, CancellationToken.None);
        }

        public Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody, CancellationToken token)
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;
            request.RequestFormat = DataFormat.Json;

            if (requestBody is string)
            {
                request.AddParameter(new Parameter
                {
                    Name = "application/json",
                    Type = ParameterType.RequestBody,
                    Value = requestBody
                });
            }
            else if (requestBody != null)
            {
                request.JsonSerializer = new RestSharpJsonSerializer(JsonSerializer.Create(this.GetSerializerSettings()));
                request.AddJsonBody(requestBody);
            }

            LogRequest(request, requestBody);
            return this._restClient.ExecuteTaskAsync(request, token).ContinueWith<JToken>(responseTask =>
            {
                var response = responseTask.Result;

                EnsureValidResponse(response);

                return response.StatusCode != HttpStatusCode.NoContent ? JToken.Parse(response.Content) : new JObject();

            });
        }

        public IRestResponse ExecuteRequest(IRestRequest request)
        {
            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);
            return response;
        }

        /// <summary>
        /// Gets time tracking information for this issue.
        /// </summary>
        public IssueTimeTrackingData GetTimeTrackingData(string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=timetracking", issueKey);
            var timeTrackingJson = ExecuteRequest(Method.GET, resource)["fields"]["timetracking"];
            return JsonConvert.DeserializeObject<IssueTimeTrackingData>(timeTrackingJson.ToString(), _serializerSettings);
        }

        private void LogRequest(RestRequest request, object body = null)
        {
            if (this._clientSettings.EnableRequestTrace)
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
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new InvalidOperationException(response.ErrorMessage);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException(response.Content);
            }
        }
        #endregion

        #region IJiraServiceClient - Supported
        public string Url
        {
            get
            {
                return _restClient.BaseUrl.ToString();
            }
        }

        public string Login(string username, string password)
        {
            // Rest api does not have a login method and does not make use of access tokens.
            return "<Unused>";
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxResults, int startAt)
        {
            var responseObj = this.ExecuteRequest(Method.POST, "rest/api/2/search", new
            {
                jql = jqlSearch,
                startAt = startAt,
                maxResults = maxResults,
            });

            var issues = (JArray)responseObj["issues"];
            return issues.Cast<JObject>().Select(issueJson => JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), this.GetSerializerSettings()).RemoteIssue).ToArray();
        }

        public Task<RemoteIssue[]> GetIssuesFromJqlSearchAsync(string jqlSearch, int maxResults, int startAt = 0)
        {
            return this.GetIssuesFromJqlSearchAsync(jqlSearch, maxResults, startAt);
        }

        public Task<RemoteIssue[]> GetIssuesFromJqlSearchAsync(string jqlSearch, int maxResults, int startAt, CancellationToken token)
        {
            var parameters = new
            {
                jql = jqlSearch,
                startAt = startAt,
                maxResults = maxResults,
            };

            return this.ExecuteRequestAsync(Method.POST, "rest/api/2/search", parameters, token).ContinueWith<RemoteIssue[]>(task =>
            {
                var issues = (JArray)task.Result["issues"];

                return issues
                    .Cast<JObject>()
                    .Select(issueJson => JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), this.GetSerializerSettings()).RemoteIssue)
                    .ToArray();
            });
        }

        public RemoteIssue CreateIssue(string token, RemoteIssue newIssue)
        {
            return CreateIssueWithParent(token, newIssue, null);
        }

        public RemoteIssue CreateIssueWithParent(string token, RemoteIssue newIssue, string parentIssueKey)
        {
            var responseObj = this.ExecuteRequest(Method.POST, "rest/api/2/issue", new RemoteIssueWrapper(newIssue, parentIssueKey));
            return this.GetIssue(token, (string)responseObj["key"]);
        }

        public RemoteVersion[] GetVersions(string token, string projectKey)
        {
            var resource = String.Format("rest/api/2/project/{0}/versions", projectKey);
            return this.ExecuteRequest<RemoteVersion[]>(Method.GET, resource);
        }

        public RemoteComponent[] GetComponents(string token, string projectKey)
        {
            var resource = String.Format("rest/api/2/project/{0}/components", projectKey);
            return this.ExecuteRequest<RemoteComponent[]>(Method.GET, resource);
        }

        public RemotePriority[] GetPriorities(string token)
        {
            return this.ExecuteRequest<RemotePriority[]>(Method.GET, "rest/api/2/priority");
        }

        public Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync()
        {
            return this.ExecuteRequestAsync<RemotePriority[]>(Method.GET, "rest/api/2/priority").ContinueWith(task =>
            {
                return task.Result.Select(p => new IssuePriority(p));
            });
        }

        public RemoteField[] GetCustomFields(string token)
        {
            if (this._customFields == null)
            {
                this._customFields = this.ExecuteRequest<RemoteField[]>(Method.GET, "rest/api/2/field")
                    .Where(f => f.IsCustomField).ToArray();
            }

            return this._customFields;
        }

        public Task<IEnumerable<CustomField>> GetCustomFieldsAsync()
        {
            if (this._customFields == null)
            {
                return this.ExecuteRequestAsync<RemoteField[]>(Method.GET, "rest/api/2/field").ContinueWith<IEnumerable<CustomField>>(task =>
                {
                    this._customFields = task.Result.Where(f => f.IsCustomField).ToArray();
                    return this._customFields.Select(f => new CustomField(f));
                });
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<CustomField>>();
                taskSource.SetResult(this._customFields.Select(f => new CustomField(f)));
                return taskSource.Task;
            }
        }

        public RemoteField[] GetFieldsForEdit(string token, string key)
        {
            // TODO: this needs to be a different call.
            return this.GetCustomFields(token);
        }

        public RemoteIssue GetIssue(string token, string issueId)
        {
            var resource = String.Format("rest/api/2/issue/{0}", issueId);
            return this.ExecuteRequest<RemoteIssueWrapper>(Method.GET, resource).RemoteIssue;
        }

        public RemoteIssue GetIssueById(string token, string issueId)
        {
            return GetIssue(token, issueId);
        }

        public RemoteProject[] GetProjects(string token)
        {
            return this.ExecuteRequest<RemoteProject[]>(Method.GET, "rest/api/2/project");
        }

        public RemoteIssueType[] GetIssueTypes(string token, string projectId)
        {
            return this.ExecuteRequest<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype");
        }

        public Task<IEnumerable<IssueType>> GetIssueTypesAsync()
        {
            return this.ExecuteRequestAsync<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype").ContinueWith(task =>
            {
                return task.Result.Select(t => new IssueType(t));
            });
        }

        public DateTime GetResolutionDateByKey(string token, string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=resolutiondate", issueKey);
            var issueJson = this.ExecuteRequest(Method.GET, resource);
            var fields = issueJson["fields"];
            var resolutionDate = fields["resolutiondate"];

            return resolutionDate.Type == JTokenType.Null ? DateTime.MinValue : (DateTime)resolutionDate;
        }

        public RemoteFilter[] GetFavouriteFilters(string token)
        {
            return this.ExecuteRequest<RemoteFilter[]>(Method.GET, "rest/api/2/filter/favourite");
        }

        public Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync()
        {
            return this.GetFavouriteFiltersAsync(CancellationToken.None);
        }

        public Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync(CancellationToken token)
        {
            return this.ExecuteRequestAsync<IEnumerable<JiraFilter>>(Method.GET, "rest/api/2/filter/favourite", null, token);
        }

        public RemoteIssue[] GetIssuesFromFilterWithLimit(string token, string filterId, int offset, int maxResults)
        {
            var resource = String.Format("rest/api/2/filter/{0}", filterId);
            var jql = this.ExecuteRequest(Method.GET, resource)["jql"].ToString();

            return this.GetIssuesFromJqlSearch(token, jql, maxResults, offset);
        }

        public RemoteStatus[] GetStatuses(string token)
        {
            return this.ExecuteRequest<RemoteStatus[]>(Method.GET, "rest/api/2/status");
        }

        public Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync()
        {
            return this.ExecuteRequestAsync<RemoteStatus[]>(Method.GET, "rest/api/2/status").ContinueWith(task =>
            {
                return task.Result.Select(s => new IssueStatus(s));
            });
        }

        public RemoteResolution[] GetResolutions(string token)
        {
            return this.ExecuteRequest<RemoteResolution[]>(Method.GET, "rest/api/2/resolution");
        }

        public Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync()
        {
            return this.ExecuteRequestAsync<RemoteResolution[]>(Method.GET, "rest/api/2/resolution").ContinueWith(task =>
            {
                return task.Result.Select(r => new IssueResolution(r));
            });
        }

        public RemoteComment[] GetCommentsFromIssue(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=comment", key);
            var issueJson = this.ExecuteRequest(Method.GET, resource);
            var comments = issueJson["fields"]["comment"]["comments"];

            return JsonConvert.DeserializeObject<RemoteComment[]>(comments.ToString(), this.GetSerializerSettings());
        }

        public void AddComment(string token, string key, RemoteComment comment)
        {
            var resource = String.Format("rest/api/2/issue/{0}/comment", key);
            var responseObj = this.ExecuteRequest(Method.POST, resource, comment);
        }

        public RemoteAttachment[] GetAttachmentsFromIssue(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=attachment", key);
            var issueJson = this.ExecuteRequest(Method.GET, resource);
            var attachments = issueJson["fields"]["attachment"];

            return JsonConvert.DeserializeObject<RemoteAttachment[]>(attachments.ToString(), this.GetSerializerSettings());
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

            this.ExecuteRequest(request);

            return true;
        }

        public RemoteNamedObject[] GetAvailableActions(string token, string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}/transitions", issueKey);
            var transitions = this.ExecuteRequest(Method.GET, resource)["transitions"];
            return JsonConvert.DeserializeObject<RemoteNamedObject[]>(transitions.ToString(), this.GetSerializerSettings());
        }

        public void AddLabels(string token, RemoteIssue remoteIssue, string[] labels)
        {
            var resource = String.Format("rest/api/2/issue/{0}", remoteIssue.key);
            this.ExecuteRequest(Method.PUT, resource, new
            {
                fields = new
                {
                    labels = labels
                }

            });
        }

        private JObject BuildFieldsObjectFromIssue(RemoteIssue remoteIssue, RemoteFieldValue[] remoteFields)
        {
            var issueWrapper = new RemoteIssueWrapper(remoteIssue);
            var issueJson = JsonConvert.SerializeObject(issueWrapper, this.GetSerializerSettings());
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

            this.ExecuteRequest(Method.PUT, resource, new { fields = fields });

            return this.GetIssue(token, remoteIssue.key);
        }

        public RemoteIssue ProgressWorkflowAction(string token, RemoteIssue remoteIssue, string actionId, RemoteFieldValue[] remoteFields)
        {
            var resource = String.Format("rest/api/2/issue/{0}/transitions", remoteIssue.key);
            var fields = BuildFieldsObjectFromIssue(remoteIssue, remoteFields);
            this.ExecuteRequest(Method.POST, resource, new
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
            var versions = this.ExecuteRequest(Method.DELETE, resource);
        }

        public RemoteIssueType[] GetSubTaskIssueTypes(string token)
        {
            return this.ExecuteRequest<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype")
                .Where(i => i.subTask).ToArray();
        }

        public RemoteWorklog[] GetWorkLogs(string token, string key)
        {
            var resource = String.Format("rest/api/2/issue/{0}/worklog", key);
            var worklogs = this.ExecuteRequest(Method.GET, resource)["worklogs"];

            return JsonConvert.DeserializeObject<RemoteWorklog[]>(worklogs.ToString(), this.GetSerializerSettings());
        }

        public RemoteWorklog AddWorklog(string token, string key, RemoteWorklog worklog, string queryString = null)
        {
            var resource = String.Format("rest/api/2/issue/{0}/worklog?{1}", key, queryString);
            var response = this.ExecuteRequest(Method.POST, resource, worklog);
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
            this.ExecuteRequest(Method.DELETE, resource);
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
        #endregion

        #region IJiraServiceClient - Unsupported
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
        #endregion
    }
}

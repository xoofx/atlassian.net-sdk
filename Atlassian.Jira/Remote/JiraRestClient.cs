using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Implements the service client contract using REST calls.
    /// </summary>
    internal class JiraRestClient : IJiraClient
    {
        public class Options
        {
            public JiraRestClientSettings RestClientSettings { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Url { get; set; }
            public Func<Jira> GetCurrentJiraFunc { get; set; }
        }

        private readonly RestClient _restClient;
        private readonly JiraRestClientSettings _clientSettings;
        private readonly Func<Jira> _getCurrentJiraFunc;

        private JsonSerializerSettings _serializerSettings;

        public JiraRestClient(Options options)
        {
            var url = options.Url.EndsWith("/") ? options.Url : options.Url += "/";

            this._clientSettings = options.RestClientSettings ?? new JiraRestClientSettings();
            this._getCurrentJiraFunc = options.GetCurrentJiraFunc;
            this._restClient = new RestClient(url);

            if (!String.IsNullOrEmpty(options.Username) && !String.IsNullOrEmpty(options.Password))
            {
                this._restClient.Authenticator = new HttpBasicAuthenticator(options.Username, options.Password);
            }
        }

        public JsonSerializerSettings GetSerializerSettings()
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
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
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

                return GetValidJsonFromResponse(response);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public IRestResponse ExecuteRequest(IRestRequest request)
        {
            var response = this._restClient.Execute(request);
            GetValidJsonFromResponse(response);
            return response;
        }

        public Task<Issue> GetIssueAsync(string issueKey, CancellationToken token)
        {
            var jira = this._getCurrentJiraFunc();
            var resource = String.Format("rest/api/2/issue/{0}", issueKey);
            return this.ExecuteRequestAsync<RemoteIssueWrapper>(Method.GET, resource, null, token).ContinueWith(task =>
            {
                return new Issue(jira, task.Result.RemoteIssue);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<Issue> UpdateIssueAsync(Issue issue, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}", issue.Key.Value);
            var fieldProvider = issue as IRemoteIssueFieldProvider;
            var remoteFields = fieldProvider.GetRemoteFields();
            var remoteIssue = issue.ToRemote();
            var fields = BuildFieldsObjectFromIssue(remoteIssue, remoteFields);

            return this.ExecuteRequestAsync(Method.PUT, resource, new { fields = fields }, token).ContinueWith(task =>
            {
                return this.GetIssueAsync(issue.Key.Value, token);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }

        public Task<Issue> CreateIssueAsyc(Issue issue, CancellationToken token)
        {
            var remoteIssueWrapper = new RemoteIssueWrapper(issue.ToRemote(), issue.ParentIssueKey);

            return this.ExecuteRequestAsync(Method.POST, "rest/api/2/issue", remoteIssueWrapper, token).ContinueWith(task =>
            {
                return this.GetIssueAsync((string)task.Result["key"], token);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }

        public Task<Issue> ExecuteIssueWorkflowActionAsync(Issue issue, string actionId, WorkflowTransitionUpdates updates, CancellationToken token)
        {
            updates = updates ?? new WorkflowTransitionUpdates();

            var resource = String.Format("rest/api/2/issue/{0}/transitions", issue.Key.Value);
            var fieldProvider = issue as IRemoteIssueFieldProvider;
            var remoteFields = fieldProvider.GetRemoteFields();
            var remoteIssue = issue.ToRemote();
            var fields = BuildFieldsObjectFromIssue(remoteIssue, remoteFields);
            var updatesObject = new JObject();

            if (!String.IsNullOrEmpty(updates.Comment))
            {
                updatesObject.Add("comment", new JArray(new JObject[]
                {
                    new JObject(new JProperty("add",
                        new JObject(new JProperty("body", updates.Comment))))
                }));
            }

            var requestBody = new
            {
                transition = new
                {
                    id = actionId
                },
                update = updatesObject,
                fields = fields
            };

            return this.ExecuteRequestAsync(Method.POST, resource, requestBody, token).ContinueWith(task =>
            {
                return this.GetIssueAsync(issue.Key.Value, token);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }

        public Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues = null, int startAt = 0)
        {
            return this.GetIssuesFromJqlAsync(jql, maxIssues, startAt, CancellationToken.None);
        }

        public Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int? maxIssues, int startAt, CancellationToken token)
        {
            var jira = this._getCurrentJiraFunc();
            var parameters = new
            {
                jql = jql,
                startAt = startAt,
                maxResults = maxIssues ?? jira.MaxIssuesPerRequest,
            };

            return this.ExecuteRequestAsync(Method.POST, "rest/api/2/search", parameters, token).ContinueWith<IPagedQueryResult<Issue>>(task =>
            {
                var issues = task.Result["issues"]
                    .Cast<JObject>()
                    .Select(issueJson =>
                    {
                        var remoteIssue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(issueJson.ToString(), this.GetSerializerSettings()).RemoteIssue;
                        return new Issue(jira, remoteIssue);
                    });

                return PagedQueryResult<Issue>.FromJson((JObject)task.Result, issues);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        private Task<IDictionary<string, Issue>> GetIssuesMapAsync(string[] issueKeys, CancellationToken token)
        {
            if (issueKeys.Any())
            {
                var distinctKeys = issueKeys.Distinct();
                var jql = String.Format("key in ({0})", String.Join(",", distinctKeys));
                return this.GetIssuesFromJqlAsync(jql, distinctKeys.Count()).ContinueWith<IDictionary<string, Issue>>(task =>
                {
                    return task.Result.ToDictionary<Issue, string>(i => i.Key.Value);
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IDictionary<string, Issue>>();
                taskSource.SetResult(new Dictionary<string, Issue>());
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<IssueLink>> GetIssueLinksAsync(Issue issue, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=issuelinks,created", issue.Key.Value);
            var serializerSettings = this.GetSerializerSettings();

            if (String.IsNullOrEmpty(issue.OriginalRemoteIssue.key))
            {
                throw new InvalidOperationException("Unable to get issue links issues, issue has not been created.");
            }

            return this.ExecuteRequestAsync(Method.GET, resource, null, token).ContinueWith(issueLinksTask =>
            {
                var issueLinksJson = issueLinksTask.Result["fields"]["issuelinks"];

                if (issueLinksJson == null)
                {
                    throw new InvalidOperationException("There is no 'issueLinks' field on the issue data, make sure issue linking is turned on in JIRA.");
                }

                var issueLinks = issueLinksJson.Cast<JObject>();
                var linkedIssueKeys = issueLinks.Select(issueLink =>
                    {
                        var issueJson = issueLink["outwardIssue"] ?? issueLink["inwardIssue"];
                        return issueJson["key"].Value<string>();
                    }).ToArray();

                return this.GetIssuesMapAsync(linkedIssueKeys, token).ContinueWith(issuesTask =>
                {
                    var issuesMap = issuesTask.Result;
                    return issueLinks.Select(issueLink =>
                    {
                        var linkType = JsonConvert.DeserializeObject<IssueLinkType>(issueLink["type"].ToString(), serializerSettings);
                        var outwardIssue = issueLink["outwardIssue"];
                        var inwardIssue = issueLink["inwardIssue"];
                        var outwardIssueKey = outwardIssue != null ? (string)outwardIssue["key"] : null;
                        var inwardIssueKey = inwardIssue != null ? (string)inwardIssue["key"] : null;
                        return new IssueLink(
                            linkType,
                            outwardIssueKey == null ? issue : issuesMap[outwardIssueKey],
                            inwardIssueKey == null ? issue : issuesMap[inwardIssueKey]);
                    });
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }

        public IssueTimeTrackingData GetTimeTrackingData(string issueKey)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=timetracking", issueKey);
            var timeTrackingJson = ExecuteRequest(Method.GET, resource)["fields"]["timetracking"];
            return JsonConvert.DeserializeObject<IssueTimeTrackingData>(timeTrackingJson.ToString(), _serializerSettings);
        }

        public Task<IDictionary<String, IssueFieldEditMetadata>> GetIssueFieldsEditMetadataAsync(string issueKey, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/issue/{0}/editmeta", issueKey);

            return ExecuteRequestAsync(Method.GET, resource, null, token).ContinueWith<IDictionary<String, IssueFieldEditMetadata>>(task =>
            {
                var dict = new Dictionary<string, IssueFieldEditMetadata>();
                var serializer = JsonSerializer.Create(this.GetSerializerSettings());
                JObject fields = task.Result["fields"].Value<JObject>();

                foreach (var prop in fields.Properties())
                {
                    var fieldName = (prop.Value["name"] ?? prop.Name).ToString();
                    dict.Add(fieldName, prop.Value.ToObject<IssueFieldEditMetadata>(serializer));
                }

                return dict;
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public RemoteField[] GetCustomFields(string token)
        {
            try
            {
                return GetCustomFieldsAsync(CancellationToken.None).Result.Select(field => field.RemoteField).ToArray();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        public Task<IEnumerable<CustomField>> GetCustomFieldsAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.CustomFields.Any())
            {
                return this.ExecuteRequestAsync<RemoteField[]>(Method.GET, "rest/api/2/field", null, token).ContinueWith<IEnumerable<CustomField>>(task =>
                {
                    var results = task.Result.Where(f => f.IsCustomField).Select(f => new CustomField(f));
                    cache.CustomFields.AddIfMIssing(results);
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<CustomField>>();
                taskSource.SetResult(cache.CustomFields.Values);
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<JiraFilter>> GetFavouriteFiltersAsync(CancellationToken token)
        {
            return this.ExecuteRequestAsync<IEnumerable<JiraFilter>>(Method.GET, "rest/api/2/filter/favourite", null, token);
        }

        public Task<IEnumerable<IssuePriority>> GetIssuePrioritiesAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.Priorities.Any())
            {
                return this.ExecuteRequestAsync<RemotePriority[]>(Method.GET, "rest/api/2/priority", null, token).ContinueWith(task =>
                {
                    var results = task.Result.Select(p => new IssuePriority(p));
                    cache.Priorities.AddIfMIssing(results);
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<IssuePriority>>();
                taskSource.SetResult(cache.Priorities.Values);
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<IssueResolution>> GetIssueResolutionsAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.Resolutions.Any())
            {
                return this.ExecuteRequestAsync<RemoteResolution[]>(Method.GET, "rest/api/2/resolution", null, token).ContinueWith(task =>
                {
                    var results = task.Result.Select(r => new IssueResolution(r));
                    cache.Resolutions.AddIfMIssing(results);
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<IssueResolution>>();
                taskSource.SetResult(cache.Resolutions.Values);
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<IssueLinkType>> GetIssueLinkTypesAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;
            var serializerSettings = this.GetSerializerSettings();

            if (!cache.LinkTypes.Any())
            {
                return this.ExecuteRequestAsync(Method.GET, "rest/api/2/issueLinkType", null, token).ContinueWith<IEnumerable<IssueLinkType>>(task =>
                {
                    var linkTypes = task.Result["issueLinkTypes"]
                        .Cast<JObject>()
                        .Select(issueLinkJson => JsonConvert.DeserializeObject<IssueLinkType>(issueLinkJson.ToString(), serializerSettings));

                    cache.LinkTypes.AddIfMIssing(linkTypes);
                    return linkTypes;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<IssueLinkType>>();
                taskSource.SetResult(cache.LinkTypes.Values);
                return taskSource.Task;
            }
        }

        public Task LinkIssuesAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string comment, CancellationToken token)
        {
            var bodyObject = new JObject();
            bodyObject.Add("type", new JObject(new JProperty("name", linkName)));
            bodyObject.Add("inwardIssue", new JObject(new JProperty("key", inwardIssueKey)));
            bodyObject.Add("outwardIssue", new JObject(new JProperty("key", outwardIssueKey)));

            if (!String.IsNullOrEmpty(comment))
            {
                bodyObject.Add("comment", new JObject(new JProperty("body", comment)));
            }

            return this.ExecuteRequestAsync(Method.POST, "rest/api/2/issueLink", bodyObject, token);
        }

        public Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.Statuses.Any())
            {
                return this.ExecuteRequestAsync<RemoteStatus[]>(Method.GET, "rest/api/2/status", null, token).ContinueWith(task =>
                {
                    var results = task.Result.Select(s => new IssueStatus(s));
                    cache.Statuses.AddIfMIssing(results);
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<IssueStatus>>();
                taskSource.SetResult(cache.Statuses.Values);
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.IssueTypes.ContainsKey(Jira.ALL_PROJECTS_KEY))
            {
                return this.ExecuteRequestAsync<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype", null, token).ContinueWith<IEnumerable<IssueType>>(task =>
                {
                    var results = task.Result.Select(t => new IssueType(t));
                    cache.IssueTypes.AddIfMIssing(new JiraEntityDictionary<IssueType>(Jira.ALL_PROJECTS_KEY, results));
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<IssueType>>();
                taskSource.SetResult(cache.IssueTypes[Jira.ALL_PROJECTS_KEY].Values);
                return taskSource.Task;
            }
        }

        public Task<Comment> AddCommentToIssueAsync(string issueKey, Comment comment, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}/comment", issueKey);
            return this.ExecuteRequestAsync<RemoteComment>(Method.POST, resource, comment.toRemote(), token).ContinueWith(task =>
            {
                return new Comment(task.Result);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<IPagedQueryResult<Comment>> GetCommentsFromIssueAsync(string issueKey, int maxComments, int startAt, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}/comment", issueKey);
            var parameters = new
            {
                startAt = startAt,
                maxResults = maxComments,
            };

            return this.ExecuteRequestAsync(Method.GET, resource, parameters).ContinueWith<IPagedQueryResult<Comment>>(task =>
            {
                var comments = task.Result["comments"]
                    .Cast<JObject>()
                    .Select(commentJson =>
                    {
                        var remoteComment = JsonConvert.DeserializeObject<RemoteComment>(commentJson.ToString(), this.GetSerializerSettings());
                        return new Comment(remoteComment);
                    });

                return PagedQueryResult<Comment>.FromJson((JObject)task.Result, comments);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<IEnumerable<JiraNamedEntity>> GetActionsForIssueAsync(string issueKey, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}/transitions", issueKey);

            return this.ExecuteRequestAsync(Method.GET, resource, null, token).ContinueWith(task =>
            {
                var transitionsJson = task.Result["transitions"];
                var remoteTransitions = JsonConvert.DeserializeObject<RemoteNamedObject[]>(transitionsJson.ToString(), this.GetSerializerSettings());

                return remoteTransitions.Select(transition => new JiraNamedEntity(transition));
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken token)
        {
            var cache = this._clientSettings.Cache;

            if (!cache.Projects.Any())
            {
                return this.ExecuteRequestAsync<RemoteProject[]>(Method.GET, "rest/api/2/project", null, token).ContinueWith(task =>
                {
                    var jira = this._getCurrentJiraFunc();
                    var results = task.Result.Select(p => new Project(jira, p));
                    cache.Projects.AddIfMIssing(results);
                    return results;
                }, token, TaskContinuationOptions.None, TaskScheduler.Default);
            }
            else
            {
                var taskSource = new TaskCompletionSource<IEnumerable<Project>>();
                taskSource.SetResult(cache.Projects.Values);
                return taskSource.Task;
            }
        }

        public Task<IEnumerable<Attachment>> GetAttachmentsFromIssueAsync(string issueKey, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=attachment", issueKey);

            return this.ExecuteRequestAsync(Method.GET, resource, null, token).ContinueWith(task =>
            {
                var jira = this._getCurrentJiraFunc();
                var attachmentsJson = task.Result["fields"]["attachment"];
                var attachments = JsonConvert.DeserializeObject<RemoteAttachment[]>(attachmentsJson.ToString(), this.GetSerializerSettings());

                return attachments.Select(remoteAttachment => new Attachment(jira, new WebClientWrapper(), remoteAttachment));
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<string[]> GetLabelsFromIssueAsync(string issueKey, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}?fields=labels", issueKey);
            return this.ExecuteRequestAsync<RemoteIssueWrapper>(Method.GET, resource).ContinueWith(task =>
            {
                return task.Result.RemoteIssue.labelsReadOnly ?? new string[0];
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task SetLabelsForIssueAsync(string issueKey, string[] labels, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/issue/{0}", issueKey);
            return this.ExecuteRequestAsync(Method.PUT, resource, new
            {
                fields = new
                {
                    labels = labels
                }

            }, token);
        }

        public Task<IEnumerable<JiraUser>> GetWatchersFromIssueAsync(string issueKey, CancellationToken token)
        {
            var resourceUrl = String.Format("rest/api/2/issue/{0}/watchers", issueKey);

            return this.ExecuteRequestAsync(Method.GET, resourceUrl, null, token).ContinueWith(task =>
            {
                var watchersJson = task.Result["watchers"];
                return watchersJson.Select(watcherJson => JsonConvert.DeserializeObject<JiraUser>(watcherJson.ToString(), this.GetSerializerSettings()));
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public Task<IEnumerable<IssueChangeLog>> GetChangeLogsFromIssueAsync(string issueKey, CancellationToken token)
        {
            var resourceUrl = String.Format("rest/api/2/issue/{0}?fields=created&expand=changelog", issueKey);

            return this.ExecuteRequestAsync(Method.GET, resourceUrl, null, token).ContinueWith(task =>
            {
                var result = Enumerable.Empty<IssueChangeLog>();
                var changeLogs = task.Result["changelog"];
                if (changeLogs != null)
                {
                    var histories = changeLogs["histories"];
                    if (histories != null)
                    {
                        result = histories.Select(history => JsonConvert.DeserializeObject<IssueChangeLog>(history.ToString(), GetSerializerSettings()));
                    }
                }

                return result;
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
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

        private JToken GetValidJsonFromResponse(IRestResponse response)
        {
            var content = response.Content != null ? response.Content.Trim() : string.Empty;

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new InvalidOperationException(response.ErrorMessage);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError
                || response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException(String.Format("Response Content: {0}", content));
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new System.Security.Authentication.AuthenticationException(string.Format("Response Content: {0}", content));
            }
            else if (string.IsNullOrWhiteSpace(content))
            {
                return new JObject();
            }
            else if (!content.StartsWith("{") && !content.StartsWith("["))
            {
                throw new InvalidOperationException(String.Format("Response was not recognized as JSON. Content: {0}", content));
            }
            else
            {
                JToken parsedContent;

                try
                {
                    parsedContent = JToken.Parse(content);
                }
                catch (JsonReaderException ex)
                {
                    throw new InvalidOperationException(String.Format("Failed to parse response as JSON. Content: {0}", content), ex);
                }

                if (parsedContent != null && parsedContent.Type == JTokenType.Object && parsedContent["errorMessages"] != null)
                {
                    throw new InvalidOperationException(string.Format("Response reported error(s) from JIRA: {0}", parsedContent["errorMessages"].ToString()));
                }

                return parsedContent;
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
            if (String.IsNullOrEmpty(projectId))
            {
                return this.ExecuteRequest<RemoteIssueType[]>(Method.GET, "rest/api/2/issuetype");
            }
            else
            {
                var resource = String.Format("rest/api/2/project/{0}", projectId);
                var projectJson = this.ExecuteRequest(Method.GET, resource);
                var serializerSettings = GetSerializerSettings();

                return projectJson["issueTypes"]
                    .Select(issueTypeJson => JsonConvert.DeserializeObject<RemoteIssueType>(issueTypeJson.ToString(), serializerSettings))
                    .ToArray();
            }
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

        public RemoteResolution[] GetResolutions(string token)
        {
            return this.ExecuteRequest<RemoteResolution[]>(Method.GET, "rest/api/2/resolution");
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
                var issueFieldName = field.id;
                var issueFieldValue = issueFields[issueFieldName];

                if (issueFieldValue == null && issueFieldName.Equals("components", StringComparison.OrdinalIgnoreCase))
                {
                    // JIRA does not accept 'null' as a valid value for the 'components' field.
                    //   So if the components field has been cleared it must be set to empty array instead.
                    issueFieldValue = new JArray();
                }

                updateFields.Add(issueFieldName, issueFieldValue);
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

        public Task<RemoteVersion> GetVersionAsync(string versionId, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/version/{0}", versionId);
            return this.ExecuteRequestAsync<RemoteVersion>(Method.GET, resource, null, token);
        }

        public Task<RemoteVersion> UpdateVersionAsync(RemoteVersion version, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/version/{0}", version.id);
            var versionJson = JsonConvert.SerializeObject(version, GetSerializerSettings());
            return this.ExecuteRequestAsync(Method.PUT, resource, versionJson, token).ContinueWith(task =>
            {
                return this.GetVersionAsync(version.id, token);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
        }

        public Task<JiraUser> GetUser(string userName, CancellationToken token)
        {
            var resource = String.Format("rest/api/2/user?username={0}", Uri.EscapeDataString(userName));
            return this.ExecuteRequestAsync<JiraUser>(Method.GET, resource, null, token);
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

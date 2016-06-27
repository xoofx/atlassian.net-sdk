using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Class that encapsulates operations on the remote version collection of a project.
    /// </summary>
    public class ProjectVersionsEditableResource : BaseEditableResource
    {
        private readonly Project _project;
        private readonly Jira _jira;

        /// <summary>
        /// Creates a new instance of the ProjectVersionsEditableResource.
        /// </summary>
        /// <param name="jira">Instance of the Jira client.</param>
        /// <param name="project">The project on which to target the operations.</param>
        public ProjectVersionsEditableResource(Jira jira, Project project)
        {
            _jira = jira;
            _project = project;
        }

        /// <summary>
        /// Gets the paged versions for the current project (not-cached).
        /// </summary>
        /// <param name="startAt">The page offset, if not specified then defaults to 0.</param>
        /// <param name="maxResults">How many results on the page should be included. Defaults to 50.</param>
        public IPagedQueryResult<ProjectVersion> Get(int startAt = 0, int maxResults = 50)
        {
            return ExecuteAndGuard(() => GetAsync(startAt, maxResults).Result);
        }

        /// <summary>
        /// Gets the paged versions for the current project (not-cached).
        /// </summary>
        /// <param name="startAt">The page offset, if not specified then defaults to 0.</param>
        /// <param name="maxResults">How many results on the page should be included. Defaults to 50.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IPagedQueryResult<ProjectVersion>> GetAsync(int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken))
        {
            var settings = _jira.RestClient.GetSerializerSettings();
            var resource = String.Format("rest/api/2/project/{0}/version?startAt={1}&maxResults={2}",
                _project.Key,
                startAt,
                maxResults);

            return _jira.RestClient.ExecuteRequestAsync(Method.GET, resource, null, token).ContinueWith<IPagedQueryResult<ProjectVersion>>(task =>
            {
                var versions = task.Result["values"]
                    .Cast<JObject>()
                    .Select(versionJson =>
                    {
                        var remoteVersion = JsonConvert.DeserializeObject<RemoteVersion>(versionJson.ToString(), settings);
                        return new ProjectVersion(_jira, remoteVersion);
                    });

                return PagedQueryResult<ProjectVersion>.FromJson((JObject)task.Result, versions);
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates and adds a new version to a project.
        /// </summary>
        /// <param name="projectVersion">Information of the new version.</param>
        public ProjectVersion Add(ProjectVersionCreationInfo projectVersion)
        {
            return ExecuteAndGuard(() => AddAsync(projectVersion).Result);
        }

        /// <summary>
        /// Creates and adds a new version to a project.
        /// </summary>
        /// <param name="projectVersion">Information of the new version.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<ProjectVersion> AddAsync(ProjectVersionCreationInfo projectVersion, CancellationToken token = default(CancellationToken))
        {
            var settings = _jira.RestClient.GetSerializerSettings();
            var serializer = JsonSerializer.Create(settings);
            var resource = "/rest/api/2/version";
            var requestBody = JToken.FromObject(projectVersion, serializer);

            requestBody["project"] = _project.Key;

            return _jira.RestClient.ExecuteRequestAsync<RemoteVersion>(Method.POST, resource, requestBody, token).ContinueWith(task =>
            {
                var version = new ProjectVersion(_jira, task.Result);
                var cacheEntry = new JiraEntityDictionary<ProjectVersion>(_project.Key, new ProjectVersion[1] { version });

                if (!_jira.Cache.Versions.AddIfMIssing(cacheEntry))
                {
                    // If there was already an entry for the project, add the version to its list.
                    _jira.Cache.Versions[_project.Key].AddIfMIssing(version);
                }

                return version;
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Deletes a version from a project.
        /// </summary>
        /// <param name="versionName">Name of the version to remove.</param>
        /// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        /// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        public void Delete(string versionName, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null)
        {
            ExecuteAndGuard(() => DeleteAsync(versionName).Wait());
        }

        /// <summary>
        /// Deletes a version from a project.
        /// </summary>
        /// <param name="versionName">Name of the version to remove.</param>
        /// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        /// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task DeleteAsync(string versionName, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null, CancellationToken token = default(CancellationToken))
        {
            var version = _jira.GetProjectVersions(_project.Key).First(v => v.Name.Equals(versionName, StringComparison.OrdinalIgnoreCase));

            var resource = String.Format("/rest/api/2/version/{0}?{1}&{2}",
                version.Id,
                String.IsNullOrEmpty(moveFixIssuesTo) ? null : "moveFixIssuesTo=" + Uri.EscapeDataString(moveFixIssuesTo),
                String.IsNullOrEmpty(moveAffectedIssuesTo) ? null : "moveAffectedIssuesTo=" + Uri.EscapeDataString(moveAffectedIssuesTo));

            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token).ContinueWith(task =>
            {
                JiraEntityDictionary<ProjectVersion> cacheEntry;

                if (_jira.Cache.Versions.TryGetValue(_project.Key, out cacheEntry) && cacheEntry.ContainsKey(version.Id))
                {
                    cacheEntry.Remove(version.Id);
                }
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

       
    }
}

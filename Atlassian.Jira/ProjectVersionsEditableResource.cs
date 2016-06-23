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
    public class ProjectVersionsEditableResource
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
            return GetAsync(startAt, maxResults).Result;
        }

        /// <summary>
        /// Gets the paged versions for the current project (not-cached).
        /// </summary>
        /// <param name="startAt">The page offset, if not specified then defaults to 0.</param>
        /// <param name="maxResults">How many results on the page should be included. Defaults to 50.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IPagedQueryResult<ProjectVersion>> GetAsync(int startAt = 0, int maxResults = 50, CancellationToken token = default(CancellationToken))
        {
            return _project.GetPagedVersionsAsync(startAt, maxResults);
        }

        /// <summary>
        /// Creates and adds a new version to a project.
        /// </summary>
        /// <param name="projectVersion">Information of the new version.</param>
        public ProjectVersion Add(ProjectVersionCreationInfo projectVersion)
        {
            return AddAsync(projectVersion).Result;
        }

        /// <summary>
        /// Creates and adds a new version to a project.
        /// </summary>
        /// <param name="projectVersion">Information of the new version.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<ProjectVersion> AddAsync(ProjectVersionCreationInfo projectVersion, CancellationToken token = default(CancellationToken))
        {
            return _project.AddVersionAsync(projectVersion, token);
        }

        /// <summary>
        /// Deletes a version from a project.
        /// </summary>
        /// <param name="versionName">Name of the version to remove.</param>
        /// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        /// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
        public void Delete(string versionName, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null)
        {
            DeleteAsync(versionName).Wait();
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
            return _project.DeleteVersionAsync(versionName, moveFixIssuesTo, moveAffectedIssuesTo, token);
        }
    }
}

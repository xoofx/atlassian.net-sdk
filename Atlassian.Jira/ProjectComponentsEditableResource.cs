using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// Class that encapsulates operations on the remote components collection of a project.

    public class ProjectComponentsEditableResource
    {
        private readonly Project _project;
        private readonly Jira _jira;

        /// <summary>
        /// Creates a new instance of the ProjectComponentsEditableResource.
        /// </summary>
        /// <param name="jira">Instance of the Jira client.</param>
        /// <param name="project">The project on which to target the operations.</param>
        public ProjectComponentsEditableResource(Jira jira, Project project)
        {
            _jira = jira;
            _project = project;
        }

        /// <summary>
        /// Gets the components for the current project.
        /// </summary>
        public IEnumerable<ProjectComponent> Get()
        {
            return GetAsync().Result;
        }

        /// <summary>
        /// Gets the components for the current project.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<ProjectComponent>> GetAsync(CancellationToken token = default(CancellationToken))
        {
            return _project.GetComponetsAsync(token);
        }

        /// <summary>
        /// Creates and adds a new component the current project.
        /// </summary>
        /// <param name="projectComponent">Information of the new component.</param>
        public ProjectComponent Add(ProjectComponentCreationInfo projectComponent)
        {
            return AddAsync(projectComponent).Result;
        }

        /// <summary>
        /// Creates and adds a new component to a project.
        /// </summary>
        /// <param name="projectComponent">Information of the new component.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<ProjectComponent> AddAsync(ProjectComponentCreationInfo projectComponent, CancellationToken token = default(CancellationToken))
        {
            return _project.AddComponentAsync(projectComponent, token);
        }

        /// <summary>
        /// Deletes a component from a project.
        /// </summary>
        /// <param name="componentName">Name of the component to remove.</param>
        /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
        public void Delete(string componentName, string moveIssuesTo = null)
        {
            DeleteAsync(componentName, moveIssuesTo).Wait();
        }

        /// <summary>
        /// Deletes a component from a project.
        /// </summary>
        /// <param name="componentName">Name of the component to remove.</param>
        /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task DeleteAsync(string componentName, string moveIssuesTo = null, CancellationToken token = default(CancellationToken))
        {
            return _project.DeleteComponentAsync(componentName, moveIssuesTo, token);
        }
    }
}
